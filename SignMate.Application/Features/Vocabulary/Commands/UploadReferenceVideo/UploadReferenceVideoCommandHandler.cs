using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Vocabulary;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Vocabulary.Commands.UploadReferenceVideo;

/// <summary>
/// Handler cho <see cref="UploadReferenceVideoCommand"/>: lưu video lên blob storage, tạo bản ghi
/// yêu cầu (trạng thái Pending) rồi đẩy vào hàng đợi xử lý nền để AI tách keypoint.
/// Thứ tự: upload blob trước (nếu lỗi thì chưa ghi DB) → lưu request → enqueue sau khi đã có Id.
/// </summary>
public class UploadReferenceVideoCommandHandler
    : IRequestHandler<UploadReferenceVideoCommand, UploadReferenceResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobService _blobService;
    private readonly IVideoProcessingQueue _processingQueue;

    public UploadReferenceVideoCommandHandler(
        IUnitOfWork unitOfWork, IBlobService blobService, IVideoProcessingQueue processingQueue)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _processingQueue = processingQueue;
    }

    /// <inheritdoc />
    public async Task<UploadReferenceResultDto> Handle(
        UploadReferenceVideoCommand command, CancellationToken cancellationToken)
    {
        var signExists = await _unitOfWork.Repository<Sign>().GetByIdAsync(command.SignId)
            ?? throw new NotFoundException(nameof(Sign), command.SignId);

        var contentType = string.IsNullOrWhiteSpace(command.ContentType) ? "video/mp4" : command.ContentType;
        var fileName = $"reference_{command.SignId}.mp4";
        var videoUrl = await _blobService.UploadAsync(command.Content, fileName, contentType);

        var request = new SignReferenceRequest
        {
            SignId = command.SignId,
            UploaderId = command.UploaderId,
            VideoUrl = videoUrl,
            Status = ReferenceRequestStatus.Pending
        };

        await _unitOfWork.Repository<SignReferenceRequest>().AddAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Đẩy sang background service sau khi đã có Id để worker tra cứu và gọi AI tách keypoint.
        await _processingQueue.QueueBackgroundWorkItemAsync(request.Id);

        return new UploadReferenceResultDto { RequestId = request.Id };
    }
}

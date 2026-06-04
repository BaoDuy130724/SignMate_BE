using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Vocabulary.Commands.ApproveReferenceRequest;

/// <summary>
/// Handler cho <see cref="ApproveReferenceRequestCommand"/>: chỉ duyệt khi yêu cầu đã ở trạng thái
/// ReadyForReview và AI đã trả về keypoint. Cập nhật đồng thời từ vựng (keypoint + video) và trạng thái
/// yêu cầu trong cùng một SaveChanges → đảm bảo Atomicity.
/// </summary>
public class ApproveReferenceRequestCommandHandler : IRequestHandler<ApproveReferenceRequestCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveReferenceRequestCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(ApproveReferenceRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _unitOfWork.Repository<SignReferenceRequest>().Query()
            .Include(x => x.Sign)
            .FirstOrDefaultAsync(x => x.Id == command.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(SignReferenceRequest), command.RequestId);

        if (request.Status != ReferenceRequestStatus.ReadyForReview)
            throw new BadRequestException("Yêu cầu chưa sẵn sàng để duyệt.");

        if (string.IsNullOrEmpty(request.ExtractedKeypoints))
            throw new BadRequestException("AI chưa trả về keypoint hoặc đã xử lý thất bại.");

        // Áp keypoint mẫu cùng video tham chiếu vào từ vựng đích.
        request.Sign.ReferenceKeypointData = request.ExtractedKeypoints;
        request.Sign.VideoUrl = request.VideoUrl;

        request.Status = ReferenceRequestStatus.Approved;
        request.ReviewedById = command.ReviewerId;
        request.ReviewedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

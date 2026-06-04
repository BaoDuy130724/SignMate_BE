using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Vocabulary;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Vocabulary.Queries.GetPendingReferenceRequests;

/// <summary>
/// Handler liệt kê các yêu cầu bơm video ở trạng thái ReadyForReview (AI đã tách keypoint xong),
/// kèm tên từ vựng và người tải lên để quản trị viên đối chiếu trước khi duyệt.
/// </summary>
public class GetPendingReferenceRequestsQueryHandler
    : IRequestHandler<GetPendingReferenceRequestsQuery, List<PendingReferenceRequestDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingReferenceRequestsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<PendingReferenceRequestDto>> Handle(
        GetPendingReferenceRequestsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<SignReferenceRequest>().Query()
            .AsNoTracking()
            .Where(x => x.Status == ReferenceRequestStatus.ReadyForReview)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PendingReferenceRequestDto
            {
                Id = x.Id,
                SignName = x.Sign.Word,
                UploaderName = x.Uploader.FullName,
                VideoUrl = x.VideoUrl,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}

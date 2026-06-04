using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Vocabulary.Commands.RejectReferenceRequest;

/// <summary>
/// Handler cho <see cref="RejectReferenceRequestCommand"/>: đánh dấu yêu cầu là bị từ chối kèm lý do.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class RejectReferenceRequestCommandHandler : IRequestHandler<RejectReferenceRequestCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectReferenceRequestCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(RejectReferenceRequestCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<SignReferenceRequest>();
        var request = await repo.GetByIdAsync(command.RequestId)
            ?? throw new NotFoundException(nameof(SignReferenceRequest), command.RequestId);

        request.Status = ReferenceRequestStatus.Rejected;
        request.ReviewedById = command.ReviewerId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewComment = command.Reason;

        repo.Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

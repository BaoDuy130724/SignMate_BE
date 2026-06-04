using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Vocabulary.Commands.SetSignReference;

/// <summary>
/// Handler cho <see cref="SetSignReferenceCommand"/>: gán chuỗi keypoint mẫu cho từ vựng.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class SetSignReferenceCommandHandler : IRequestHandler<SetSignReferenceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetSignReferenceCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(SetSignReferenceCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Sign>();
        var sign = await repo.GetByIdAsync(command.Request.SignId)
            ?? throw new NotFoundException(nameof(Sign), command.Request.SignId);

        sign.ReferenceKeypointData = command.Request.ReferenceKeypointData;
        repo.Update(sign);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

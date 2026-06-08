using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Commands.DeleteCenter;

/// <summary>
/// Handler cho <see cref="DeleteCenterCommand"/>: xóa trung tâm nếu tồn tại, ngược lại ném 404.
/// </summary>
public class DeleteCenterCommandHandler : IRequestHandler<DeleteCenterCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCenterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCenterCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<CenterEntity>();
        var center = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Center", command.Id);

        repo.Delete(center);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

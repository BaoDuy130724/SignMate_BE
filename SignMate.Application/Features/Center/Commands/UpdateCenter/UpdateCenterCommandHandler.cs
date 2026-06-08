using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Commands.UpdateCenter;

/// <summary>
/// Handler cho <see cref="UpdateCenterCommand"/>: ghi đè thông tin trung tâm nếu tồn tại, ngược lại 404.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class UpdateCenterCommandHandler : IRequestHandler<UpdateCenterCommand, CenterDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCenterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<CenterDto> Handle(UpdateCenterCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<CenterEntity>();
        var center = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException("Center", command.Id);

        var dto = command.Request;
        center.Name = dto.Name;
        center.ContactPerson = dto.ContactPerson;
        center.Phone = dto.Phone;
        center.Email = dto.Email;
        center.MaxSeats = dto.MaxSeats;
        center.IsActive = dto.IsActive;

        repo.Update(center);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = center.Id;
        return dto;
    }
}

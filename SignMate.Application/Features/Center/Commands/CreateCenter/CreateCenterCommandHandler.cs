using MediatR;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Commands.CreateCenter;

/// <summary>
/// Handler cho <see cref="CreateCenterCommand"/>: tạo trung tâm mới ở trạng thái hoạt động.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class CreateCenterCommandHandler : IRequestHandler<CreateCenterCommand, CenterDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCenterCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<CenterDto> Handle(CreateCenterCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Request;
        var center = new CenterEntity
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            MaxSeats = dto.MaxSeats,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CenterEntity>().AddAsync(center);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        dto.Id = center.Id;
        dto.IsActive = true;
        return dto;
    }
}

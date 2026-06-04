using MediatR;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.CreateClass;

/// <summary>
/// Handler cho <see cref="CreateClassCommand"/>: tạo lớp học mới gắn với trung tâm và giáo viên.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, ClassDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateClassCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<ClassDto> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        var newClass = new Class
        {
            CenterId = command.CenterId,
            Name = command.Request.Name,
            TeacherId = command.Request.TeacherId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Class>().AddAsync(newClass);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = newClass.Id,
            Name = newClass.Name,
            TeacherId = newClass.TeacherId
        };
    }
}

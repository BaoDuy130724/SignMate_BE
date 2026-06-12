using MediatR;
using SignMate.Application.Common.Exceptions;
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
    private readonly ICurrentUser _currentUser;

    public CreateClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<ClassDto> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != command.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền tạo lớp học cho trung tâm khác.");
        }

        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(command.Request.TeacherId);
        if (teacher == null || teacher.CenterId != command.CenterId || teacher.Role != UserRole.Teacher)
        {
            throw new BadRequestException("Giáo viên chỉ định không hợp lệ hoặc không thuộc trung tâm này.");
        }
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

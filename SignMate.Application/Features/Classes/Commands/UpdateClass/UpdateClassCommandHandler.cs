using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.UpdateClass;

/// <summary>
/// Handler cho <see cref="UpdateClassCommand"/>: cập nhật Name và TeacherId của lớp học.
/// </summary>
public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, ClassDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateClassCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<ClassDto> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var cls = await _unitOfWork.Repository<Class>().Query()
            .FirstOrDefaultAsync(c => c.Id == command.ClassId && c.CenterId == command.CenterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Class), command.ClassId);

        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(command.Request.TeacherId)
            ?? throw new BadRequestException("Giáo viên không tồn tại.");

        if (teacher.CenterId != command.CenterId || teacher.Role != UserRole.Teacher)
            throw new BadRequestException("Giáo viên không hợp lệ hoặc không thuộc trung tâm này.");

        cls.Name = command.Request.Name;
        cls.TeacherId = command.Request.TeacherId;

        _unitOfWork.Repository<Class>().Update(cls);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = cls.Id,
            Name = cls.Name,
            TeacherId = cls.TeacherId,
            TeacherName = teacher.FullName
        };
    }
}

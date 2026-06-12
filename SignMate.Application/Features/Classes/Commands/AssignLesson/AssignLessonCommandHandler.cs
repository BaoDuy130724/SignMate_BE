using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.AssignLesson;

/// <summary>
/// Handler cho <see cref="AssignLessonCommand"/>: tạo bản ghi giao bài cho lớp.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class AssignLessonCommandHandler : IRequestHandler<AssignLessonCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AssignLessonCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(AssignLessonCommand command, CancellationToken cancellationToken)
    {
        var cls = await _unitOfWork.Repository<Class>().Query()
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);
        if (cls == null)
            throw new NotFoundException("Lớp học không tồn tại.");

        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && cls.CenterId != _currentUser.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này cho trung tâm khác.");
        }

        if (_currentUser.Role == UserRole.Teacher.ToString() && cls.TeacherId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn không có quyền giao bài học cho lớp học do giáo viên khác phụ trách.");
        }
        await _unitOfWork.Repository<LessonAssignment>().AddAsync(new LessonAssignment
        {
            ClassId = command.ClassId,
            LessonId = command.Request.LessonId,
            AssignedBy = command.TeacherId,
            AssignedAt = DateTime.UtcNow,
            DueDate = command.Request.DueDate
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

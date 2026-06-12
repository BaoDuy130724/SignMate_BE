using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.AddStudents;

/// <summary>
/// Handler cho <see cref="AddStudentsCommand"/>: thêm các học viên chưa có vào lớp (bỏ qua học viên
/// đã thuộc lớp để tránh trùng). Toàn bộ thêm mới nằm trong một SaveChanges nên atomic.
/// </summary>
public class AddStudentsCommandHandler : IRequestHandler<AddStudentsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AddStudentsCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(AddStudentsCommand command, CancellationToken cancellationToken)
    {
        var cls = await _unitOfWork.Repository<Class>().Query()
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);
        if (cls == null)
            throw new NotFoundException("Lớp học không tồn tại.");

        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && cls.CenterId != _currentUser.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này cho trung tâm khác.");
        }

        var invalidStudentsCount = await _unitOfWork.Repository<User>().Query()
            .CountAsync(u => command.Request.StudentIds.Contains(u.Id) && (u.CenterId != cls.CenterId || u.Role != UserRole.Student), cancellationToken);
        if (invalidStudentsCount > 0)
        {
            throw new BadRequestException("Một hoặc nhiều học viên không hợp lệ hoặc không thuộc trung tâm của bạn.");
        }
        var repo = _unitOfWork.Repository<ClassStudent>();

        // Lấy sẵn các học viên đã có trong lớp để lọc trùng trong một truy vấn.
        var existingStudentIds = await repo.Query()
            .Where(cs => cs.ClassId == command.ClassId && command.Request.StudentIds.Contains(cs.StudentId))
            .Select(cs => cs.StudentId)
            .ToListAsync(cancellationToken);

        var toAdd = command.Request.StudentIds.Distinct().Except(existingStudentIds);
        foreach (var studentId in toAdd)
            await repo.AddAsync(new ClassStudent { ClassId = command.ClassId, StudentId = studentId });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.RemoveStudent;

/// <summary>
/// Handler cho <see cref="RemoveStudentCommand"/>: gỡ học viên khỏi lớp học.
/// </summary>
public class RemoveStudentCommandHandler : IRequestHandler<RemoveStudentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public RemoveStudentCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(RemoveStudentCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != command.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền thực hiện thao tác này cho trung tâm khác.");
        }
        var classExists = await _unitOfWork.Repository<Class>().Query()
            .AnyAsync(c => c.Id == command.ClassId && c.CenterId == command.CenterId, cancellationToken);
        if (!classExists)
            throw new NotFoundException("Lớp học không tồn tại trong trung tâm này.");

        var classStudent = await _unitOfWork.Repository<ClassStudent>().Query()
            .FirstOrDefaultAsync(cs => cs.ClassId == command.ClassId && cs.StudentId == command.StudentId, cancellationToken);
        if (classStudent == null)
            throw new NotFoundException("Học viên không thuộc lớp học này.");

        _unitOfWork.Repository<ClassStudent>().Delete(classStudent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

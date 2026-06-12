using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.DeleteClass;

/// <summary>
/// Handler cho <see cref="DeleteClassCommand"/>: xóa lớp học nếu tồn tại và không còn học viên.
/// </summary>
public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteClassCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != command.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền xóa lớp học của trung tâm khác.");
        }
        var cls = await _unitOfWork.Repository<Class>().Query()
            .Include(c => c.ClassStudents)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId && c.CenterId == command.CenterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Class), command.ClassId);

        if (cls.ClassStudents.Any())
            throw new BadRequestException("Không thể xóa lớp học vì vẫn còn học viên. Hãy gỡ tất cả học viên khỏi lớp trước.");

        _unitOfWork.Repository<Class>().Delete(cls);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

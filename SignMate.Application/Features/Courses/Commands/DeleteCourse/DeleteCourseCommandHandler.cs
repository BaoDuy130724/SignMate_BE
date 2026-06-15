using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Features.Courses.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Commands.DeleteCourse;

/// <summary>
/// Handler cho <see cref="DeleteCourseCommand"/>: xóa khóa học nếu tồn tại, ngược lại ném 404.
/// </summary>
public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteCourseCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Course>();
        var course = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(Course), command.Id);

        ContentAccess.EnsureCanManage(course.CenterId, _currentUser);

        repo.Delete(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Features.Courses.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Lessons.Commands.DeleteLesson;

/// <summary>
/// Handler cho <see cref="DeleteLessonCommand"/>: xóa bài học nếu tồn tại, ngược lại ném 404.
/// </summary>
public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public DeleteLessonCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteLessonCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Lesson>();
        var lesson = await repo.GetByIdAsync(command.LessonId)
            ?? throw new NotFoundException(nameof(Lesson), command.LessonId);

        var courseCenterId = await _unitOfWork.Repository<Course>().Query()
            .Where(c => c.Id == lesson.CourseId).Select(c => c.CenterId)
            .FirstOrDefaultAsync(cancellationToken);
        ContentAccess.EnsureCanManage(courseCenterId, _currentUser);

        repo.Delete(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

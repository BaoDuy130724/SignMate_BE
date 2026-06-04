using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Lessons.Commands.DeleteLesson;

/// <summary>
/// Handler cho <see cref="DeleteLessonCommand"/>: xóa bài học nếu tồn tại, ngược lại ném 404.
/// </summary>
public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLessonCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteLessonCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Lesson>();
        var lesson = await repo.GetByIdAsync(command.LessonId)
            ?? throw new NotFoundException(nameof(Lesson), command.LessonId);

        repo.Delete(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Commands.DeleteCourse;

/// <summary>
/// Handler cho <see cref="DeleteCourseCommand"/>: xóa khóa học nếu tồn tại, ngược lại ném 404.
/// </summary>
public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCourseCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Course>();
        var course = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(Course), command.Id);

        repo.Delete(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

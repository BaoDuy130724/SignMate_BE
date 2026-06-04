using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Commands.UpdateCourse;

/// <summary>
/// Handler cho <see cref="UpdateCourseCommand"/>: cập nhật một phần các trường khóa học rồi trả về
/// DTO mới nhất kèm số bài học và chủ đề đại diện. Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, CourseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<CourseDto> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Course>();
        var course = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(Course), command.Id);

        var request = command.Request;
        if (request.Title is not null) course.Title = request.Title;
        if (request.Description is not null) course.Description = request.Description;
        if (request.ThumbnailUrl is not null) course.ThumbnailUrl = request.ThumbnailUrl;
        if (request.IsPublished.HasValue) course.IsPublished = request.IsPublished.Value;
        if (request.Level is not null
            && Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level))
            course.Level = level;

        repo.Update(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var lessons = _unitOfWork.Repository<Lesson>().Query().Where(l => l.CourseId == command.Id);
        var topic = await lessons.OrderBy(l => l.OrderIndex).Select(l => l.Topic).FirstOrDefaultAsync(cancellationToken) ?? "Chung";
        var lessonCount = await lessons.CountAsync(cancellationToken);

        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl,
            Level = course.Level.ToString(),
            IsPublished = course.IsPublished,
            CreatedBy = course.CreatedBy,
            CreatedAt = course.CreatedAt,
            LessonCount = lessonCount,
            Topic = topic
        };
    }
}

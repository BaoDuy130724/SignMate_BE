using MediatR;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Commands.CreateCourse;

/// <summary>
/// Handler cho <see cref="CreateCourseCommand"/>: tạo khóa học ở trạng thái nháp (chưa publish).
/// Cấp độ đã được validator đảm bảo hợp lệ nên parse an toàn ở đây.
/// </summary>
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<CourseDto> Handle(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        Enum.TryParse<CourseLevel>(request.Level, ignoreCase: true, out var level);

        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Level = level,
            IsPublished = false,
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Course>().AddAsync(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            LessonCount = 0,
            Topic = "Chung"
        };
    }
}

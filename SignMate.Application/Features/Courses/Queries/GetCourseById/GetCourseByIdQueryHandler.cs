using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Queries.GetCourseById;

/// <summary>
/// Handler đọc chi tiết khóa học cùng các bài học con (sắp theo thứ tự). Ném 404 nếu không tồn tại.
/// </summary>
public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCourseByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<CourseDetailDto> Handle(GetCourseByIdQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Course>().Query()
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CourseDetailDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailUrl = c.ThumbnailUrl,
                Level = c.Level.ToString(),
                IsPublished = c.IsPublished,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                LessonCount = c.Lessons.Count,
                Topic = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => l.Topic).FirstOrDefault() ?? "Chung",
                Lessons = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => new LessonDto
                {
                    Id = l.Id,
                    CourseId = l.CourseId,
                    Title = l.Title,
                    Topic = l.Topic,
                    OrderIndex = l.OrderIndex,
                    VideoUrl = l.VideoUrl,
                    Description = l.Description,
                    DurationSeconds = l.DurationSeconds,
                    IsPublished = l.IsPublished,
                    SignCount = l.Signs.Count
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Course), query.Id);
    }
}

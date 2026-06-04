using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Queries.GetLessonsByCourse;

/// <summary>
/// Handler liệt kê bài học con theo khóa, sắp xếp theo thứ tự hiển thị (OrderIndex).
/// </summary>
public class GetLessonsByCourseQueryHandler : IRequestHandler<GetLessonsByCourseQuery, List<LessonDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLessonsByCourseQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<LessonDto>> Handle(GetLessonsByCourseQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Lesson>().Query()
            .AsNoTracking()
            .Where(l => l.CourseId == query.CourseId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonDto
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
            })
            .ToListAsync(cancellationToken);
    }
}

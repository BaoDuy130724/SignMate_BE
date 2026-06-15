using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Queries.GetCourses;

/// <summary>
/// Handler liệt kê khóa học. Áp dụng lần lượt các bộ lọc (publish, từ khóa, cấp độ) rồi projection
/// sang DTO kèm số bài học và chủ đề đại diện của khóa.
/// </summary>
public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetCoursesQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<CourseDto>> Handle(GetCoursesQuery query, CancellationToken cancellationToken)
    {
        var coursesQuery = _unitOfWork.Repository<Course>().Query().AsNoTracking();

        if (!query.IncludeUnpublished)
            coursesQuery = coursesQuery.Where(c => c.IsPublished);

        // Phân tầng nội dung: SuperAdmin thấy tất cả; còn lại chỉ thấy khóa global
        // (CenterId == null) hoặc khóa riêng của chính center mình. B2C (CenterId == null)
        // → chỉ global; B2B/teacher/centeradmin → global + center của họ.
        if (_currentUser.Role != nameof(UserRole.SuperAdmin))
        {
            var centerId = _currentUser.CenterId;
            coursesQuery = coursesQuery.Where(c => c.CenterId == null || c.CenterId == centerId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
            coursesQuery = coursesQuery.Where(c =>
                c.Title.Contains(query.Search) ||
                (c.Description != null && c.Description.Contains(query.Search)));

        if (!string.IsNullOrWhiteSpace(query.Level)
            && Enum.TryParse<CourseLevel>(query.Level, ignoreCase: true, out var level))
            coursesQuery = coursesQuery.Where(c => c.Level == level);

        return await coursesQuery
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CourseDto
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
                CenterId = c.CenterId
            })
            .ToListAsync(cancellationToken);
    }
}

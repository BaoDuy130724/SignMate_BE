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
    private readonly ICurrentUser _currentUser;

    public GetLessonsByCourseQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<LessonDto>> Handle(GetLessonsByCourseQuery query, CancellationToken cancellationToken)
    {
        // Phân tầng nội dung (chống IDOR): chỉ trả bài học của khóa mà người dùng được xem.
        var isSuperAdmin = _currentUser.Role == nameof(UserRole.SuperAdmin);
        var centerId = _currentUser.CenterId;

        return await _unitOfWork.Repository<Lesson>().Query()
            .AsNoTracking()
            .Where(l => l.CourseId == query.CourseId)
            .Where(l => isSuperAdmin || l.Course.CenterId == null || l.Course.CenterId == centerId)
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

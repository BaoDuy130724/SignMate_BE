using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Enrollment;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Enrollments.Queries.GetMyEnrollments;

/// <summary>
/// Handler đọc danh sách enrollment của học viên. Số bài đã hoàn thành được tính trực tiếp
/// trong projection để client hiển thị thanh tiến độ (ví dụ "2/5 bài").
/// </summary>
public class GetMyEnrollmentsQueryHandler : IRequestHandler<GetMyEnrollmentsQuery, List<EnrollmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyEnrollmentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<EnrollmentDto>> Handle(GetMyEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Enrollment>().Query()
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseThumbnailUrl = e.Course.ThumbnailUrl,
                CourseLevel = e.Course.Level.ToString(),
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt,
                TotalLessons = e.Course.Lessons.Count,
                CompletedLessons = e.LessonProgresses.Count(lp => lp.Status == LessonStatus.Completed)
            })
            .ToListAsync(cancellationToken);
    }
}

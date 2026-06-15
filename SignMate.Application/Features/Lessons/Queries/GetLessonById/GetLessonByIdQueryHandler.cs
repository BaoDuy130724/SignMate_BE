using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Lessons.Queries.GetLessonById;

/// <summary>
/// Handler đọc chi tiết bài học cùng các từ vựng (sắp theo thứ tự). Trường
/// <c>ReferenceKeypointData</c> được trả về để client/giáo viên biết từ vựng đã có data mẫu AI chưa.
/// </summary>
public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, LessonDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetLessonByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<LessonDetailDto> Handle(GetLessonByIdQuery query, CancellationToken cancellationToken)
    {
        // Phân tầng nội dung (chống IDOR): chỉ xem được bài học của khóa global hoặc của center mình.
        var isSuperAdmin = _currentUser.Role == nameof(UserRole.SuperAdmin);
        var centerId = _currentUser.CenterId;

        return await _unitOfWork.Repository<Lesson>().Query()
            .AsNoTracking()
            .Where(l => l.Id == query.LessonId)
            .Where(l => isSuperAdmin || l.Course.CenterId == null || l.Course.CenterId == centerId)
            .Select(l => new LessonDetailDto
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
                SignCount = l.Signs.Count,
                Signs = l.Signs.OrderBy(s => s.OrderIndex).Select(s => new SignDto
                {
                    Id = s.Id,
                    Word = s.Word,
                    VideoUrl = s.VideoUrl,
                    ThumbnailUrl = s.ThumbnailUrl,
                    Description = s.Description,
                    OrderIndex = s.OrderIndex,
                    ReferenceKeypointData = s.ReferenceKeypointData
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Lesson), query.LessonId);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Features.Courses.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Lessons.Commands.CreateLesson;

/// <summary>
/// Handler cho <see cref="CreateLessonCommand"/>: kiểm tra khóa học tồn tại rồi tạo bài học mới.
/// </summary>
public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, LessonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateLessonCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<LessonDto> Handle(CreateLessonCommand command, CancellationToken cancellationToken)
    {
        var course = await _unitOfWork.Repository<Course>().Query()
            .Where(c => c.Id == command.CourseId)
            .Select(c => new { c.Id, c.CenterId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Course), command.CourseId);

        // Chỉ được thêm bài học vào khóa mình có quyền quản lý (center của mình / SuperAdmin).
        ContentAccess.EnsureCanManage(course.CenterId, _currentUser);

        var request = command.Request;
        var lesson = new Lesson
        {
            CourseId = command.CourseId,
            Title = request.Title,
            Topic = request.Topic,
            OrderIndex = request.OrderIndex,
            VideoUrl = request.VideoUrl,
            Description = request.Description,
            DurationSeconds = request.DurationSeconds,
            IsPublished = false
        };

        await _unitOfWork.Repository<Lesson>().AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Topic = lesson.Topic,
            OrderIndex = lesson.OrderIndex,
            VideoUrl = lesson.VideoUrl,
            Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds,
            IsPublished = lesson.IsPublished,
            SignCount = 0
        };
    }
}

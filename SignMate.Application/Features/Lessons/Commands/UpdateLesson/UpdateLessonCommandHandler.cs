using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Lessons.Commands.UpdateLesson;

/// <summary>
/// Handler cho <see cref="UpdateLessonCommand"/>: cập nhật một phần bài học rồi trả về DTO mới nhất
/// kèm số từ vựng. Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, LessonDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLessonCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<LessonDto> Handle(UpdateLessonCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Lesson>();
        var lesson = await repo.GetByIdAsync(command.LessonId)
            ?? throw new NotFoundException(nameof(Lesson), command.LessonId);

        var request = command.Request;
        if (request.Title is not null) lesson.Title = request.Title;
        if (request.Topic is not null) lesson.Topic = request.Topic;
        if (request.OrderIndex.HasValue) lesson.OrderIndex = request.OrderIndex.Value;
        if (request.VideoUrl is not null) lesson.VideoUrl = request.VideoUrl;
        if (request.Description is not null) lesson.Description = request.Description;
        if (request.DurationSeconds.HasValue) lesson.DurationSeconds = request.DurationSeconds.Value;
        if (request.IsPublished.HasValue) lesson.IsPublished = request.IsPublished.Value;

        repo.Update(lesson);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var signCount = await _unitOfWork.Repository<Sign>().Query()
            .CountAsync(s => s.LessonId == command.LessonId, cancellationToken);

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
            SignCount = signCount
        };
    }
}

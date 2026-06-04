using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Progress.Commands.UpdateLessonProgress;

/// <summary>
/// Handler cho <see cref="UpdateLessonProgressCommand"/>: tạo mới hoặc cập nhật bản ghi tiến độ bài học
/// của người dùng. Nếu bài chuyển sang <see cref="LessonStatus.Completed"/> và đã hoàn thành toàn bộ
/// bài của khóa thì đánh dấu enrollment hoàn tất. Bao nhiều bước ghi phụ thuộc nhau trong một transaction
/// để đảm bảo Atomicity (tiến độ + đánh dấu hoàn thành khóa luôn nhất quán).
/// </summary>
public class UpdateLessonProgressCommandHandler : IRequestHandler<UpdateLessonProgressCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLessonProgressCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(UpdateLessonProgressCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (!Enum.TryParse<LessonStatus>(request.Status, true, out var status))
            throw new BadRequestException($"Trạng thái không hợp lệ: {request.Status}");

        var lesson = await _unitOfWork.Repository<Lesson>().GetByIdAsync(request.LessonId)
            ?? throw new NotFoundException(nameof(Lesson), request.LessonId);

        var enrollment = await _unitOfWork.Repository<Enrollment>().Query()
            .FirstOrDefaultAsync(e => e.UserId == command.UserId && e.CourseId == lesson.CourseId, cancellationToken)
            ?? throw new BadRequestException("Bạn chưa ghi danh khóa học này.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            var progress = await _unitOfWork.Repository<LessonProgress>().Query()
                .FirstOrDefaultAsync(lp => lp.UserId == command.UserId && lp.LessonId == request.LessonId, cancellationToken);

            if (progress is null)
            {
                progress = new LessonProgress
                {
                    EnrollmentId = enrollment.Id,
                    UserId = command.UserId,
                    LessonId = request.LessonId,
                    Status = status,
                    WatchDurationSeconds = request.WatchDurationSeconds,
                    LastWatchedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<LessonProgress>().AddAsync(progress);
            }
            else
            {
                progress.Status = status;
                progress.WatchDurationSeconds += request.WatchDurationSeconds;
                progress.LastWatchedAt = DateTime.UtcNow;
            }

            if (status == LessonStatus.Completed)
            {
                var totalLessons = await _unitOfWork.Repository<Lesson>().Query()
                    .CountAsync(l => l.CourseId == lesson.CourseId, cancellationToken);
                var completedLessons = await _unitOfWork.Repository<LessonProgress>().Query()
                    .CountAsync(lp => lp.UserId == command.UserId
                        && lp.Enrollment.CourseId == lesson.CourseId
                        && lp.Status == LessonStatus.Completed, cancellationToken);

                if (completedLessons >= totalLessons)
                    enrollment.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}

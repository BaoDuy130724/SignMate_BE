using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Progress;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/progress")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly ISignMateDbContext _db;

    public ProgressController(ISignMateDbContext db) => _db = db;

    [HttpPut("lesson")]
    public async Task<IActionResult> UpdateLessonProgress([FromBody] UpdateLessonProgressRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (!Enum.TryParse<LessonStatus>(request.Status, true, out var status))
            return BadRequest(new { message = $"Invalid status: {request.Status}" });

        var lesson = await _db.Lessons.FindAsync(request.LessonId);
        if (lesson == null) return NotFound(new { message = "Lesson not found." });

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == lesson.CourseId);
        if (enrollment == null) return BadRequest(new { message = "Not enrolled in this course." });

        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == request.LessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                Id = Guid.NewGuid(), EnrollmentId = enrollment.Id,
                UserId = userId, LessonId = request.LessonId,
                Status = status, WatchDurationSeconds = request.WatchDurationSeconds,
                LastWatchedAt = DateTime.UtcNow
            };
            _db.LessonProgresses.Add(progress);
        }
        else
        {
            progress.Status = status;
            progress.WatchDurationSeconds += request.WatchDurationSeconds;
            progress.LastWatchedAt = DateTime.UtcNow;
        }

        if (status == LessonStatus.Completed)
        {
            var totalLessons = await _db.Lessons.CountAsync(l => l.CourseId == lesson.CourseId);
            var completedLessons = await _db.LessonProgresses
                .CountAsync(lp => lp.UserId == userId
                    && lp.Enrollment.CourseId == lesson.CourseId
                    && lp.Status == LessonStatus.Completed);

            if (completedLessons >= totalLessons)
                enrollment.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Progress updated." });
    }

    [HttpPut("sign")]
    public async Task<IActionResult> UpdateSignProgress([FromBody] UpdateSignProgressRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var sign = await _db.Signs.FindAsync(request.SignId);
        if (sign == null) return NotFound(new { message = "Sign not found." });

        var progress = await _db.SignProgresses
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.SignId == request.SignId);

        if (progress == null)
        {
            progress = new SignProgress
            {
                Id = Guid.NewGuid(), UserId = userId, SignId = request.SignId,
                IsMastered = request.IsMastered, AttemptCount = 1,
                LastPracticedAt = DateTime.UtcNow
            };
            _db.SignProgresses.Add(progress);
        }
        else
        {
            progress.IsMastered = request.IsMastered;
            progress.AttemptCount++;
            progress.LastPracticedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Sign progress updated." });
    }
}

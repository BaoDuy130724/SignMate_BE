using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Enrollment;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly ISignMateDbContext _db;

    public EnrollmentsController(ISignMateDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == request.CourseId))
            return Conflict(new { message = "Already enrolled." });

        var course = await _db.Courses.FindAsync(request.CourseId);
        if (course == null) return NotFound(new { message = "Course not found." });

        var enrollment = new Enrollment
        {
            Id = Guid.NewGuid(), UserId = userId,
            CourseId = request.CourseId, EnrolledAt = DateTime.UtcNow
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();
        return Created("", new { enrollmentId = enrollment.Id });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrollments = await _db.Enrollments
            .Include(e => e.Course).ThenInclude(c => c.Lessons)
            .Include(e => e.LessonProgresses)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id, CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                CourseThumbnailUrl = e.Course.ThumbnailUrl,
                CourseLevel = e.Course.Level.ToString(),
                EnrolledAt = e.EnrolledAt, CompletedAt = e.CompletedAt,
                TotalLessons = e.Course.Lessons.Count,
                CompletedLessons = e.LessonProgresses.Count(lp => lp.Status == LessonStatus.Completed)
            })
            .ToListAsync();

        return Ok(enrollments);
    }
}

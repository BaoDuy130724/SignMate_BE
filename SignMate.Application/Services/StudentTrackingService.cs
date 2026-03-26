using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.StudentTracking;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Services;

public class StudentTrackingService : IStudentTrackingService
{
    private readonly ISignMateDbContext _db;

    public StudentTrackingService(ISignMateDbContext db) => _db = db;

    public async Task<List<StudentTrackingStatsDto>> GetClassTrackingStatsAsync(Guid classId)
    {
        var students = await _db.ClassStudents
            .Where(cs => cs.ClassId == classId)
            .Select(cs => cs.Student)
            .ToListAsync();

        var result = new List<StudentTrackingStatsDto>();
        foreach (var student in students)
        {
            var attempts = await _db.PracticeSessions
                .Where(s => s.UserId == student.Id)
                .SelectMany(s => s.Attempts)
                .ToListAsync();

            double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;
            
            result.Add(new StudentTrackingStatsDto
            {
                StudentId = student.Id, FullName = student.FullName,
                AccuracyPercent = Math.Round(avgAcc, 1),
                PracticeFrequencyDays = attempts.Count // simple mock
            });
        }
        return result;
    }

    public Task<TrackingReportResponse> GenerateTrackingReportAsync(Guid centerId, DateTime from, DateTime to)
    {
        return Task.FromResult(new TrackingReportResponse
        {
            ReportUrl = "https://placeholder-url.com/report.pdf",
            GeneratedAt = DateTime.UtcNow
        });
    }
}

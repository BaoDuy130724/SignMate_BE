using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.StudentTracking;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class StudentTrackingService : IStudentTrackingService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentTrackingService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<StudentTrackingStatsDto>> GetClassTrackingStatsAsync(Guid classId)
    {
        var students = await _unitOfWork.Repository<ClassStudent>().Query()
            .Where(cs => cs.ClassId == classId)
            .Select(cs => cs.Student)
            .ToListAsync();

        var result = new List<StudentTrackingStatsDto>();
        foreach (var student in students)
        {
            var attempts = await _unitOfWork.Repository<PracticeSession>().Query()
                .Where(s => s.UserId == student.Id)
                .SelectMany(s => s.Attempts)
                .ToListAsync();

            double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;
            
            result.Add(new StudentTrackingStatsDto
            {
                StudentId = student.Id, FullName = student.FullName,
                AccuracyPercent = Math.Round(avgAcc, 1),
                PracticeFrequencyDays = attempts.Count
            });
        }
        return result;
    }

    public Task<TrackingReportResponse> GenerateTrackingReportAsync(Guid centerId, DateTime from, DateTime to)
    {
        return Task.FromResult(new TrackingReportResponse
        {
            ReportUrl = "",
            GeneratedAt = DateTime.UtcNow
        });
    }
}

using SignMate.Application.DTOs.StudentTracking;

namespace SignMate.Application.Interfaces;

public interface IStudentTrackingService
{
    Task<List<StudentTrackingStatsDto>> GetClassTrackingStatsAsync(Guid classId);
    Task<TrackingReportResponse> GenerateTrackingReportAsync(Guid centerId, DateTime from, DateTime to);
}

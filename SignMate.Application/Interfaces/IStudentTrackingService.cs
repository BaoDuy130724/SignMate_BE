using SignMate.Application.DTOs.StudentTracking;

namespace SignMate.Application.Interfaces;

public interface IStudentTrackingService
{
    Task<List<StudentTrackingStatsDto>> GetClassTrackingStatsAsync(int classId);
    Task<TrackingReportResponse> GenerateTrackingReportAsync(int centerId, DateTime from, DateTime to);
}

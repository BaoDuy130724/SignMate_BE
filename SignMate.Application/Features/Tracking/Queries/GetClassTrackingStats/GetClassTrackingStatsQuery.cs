using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.StudentTracking;

namespace SignMate.Application.Features.Tracking.Queries.GetClassTrackingStats;

/// <summary>
/// Truy vấn thống kê hiệu năng học tập của từng học viên trong một lớp —
/// <c>GET /api/tracking/classes/{classId}/students</c>.
/// </summary>
/// <param name="ClassId">Id lớp học.</param>
public record GetClassTrackingStatsQuery(int ClassId) : IQuery<List<StudentTrackingStatsDto>>;

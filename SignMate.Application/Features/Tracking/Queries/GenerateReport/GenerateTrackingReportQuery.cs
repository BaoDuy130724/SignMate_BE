using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.StudentTracking;

namespace SignMate.Application.Features.Tracking.Queries.GenerateReport;

/// <summary>
/// Truy vấn tạo báo cáo định kỳ cho trung tâm theo khoảng thời gian —
/// <c>GET /api/tracking/centers/{centerId}/reports</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm.</param>
/// <param name="From">Ngày bắt đầu chu kỳ.</param>
/// <param name="To">Ngày kết thúc chu kỳ.</param>
public record GenerateTrackingReportQuery(int CenterId, DateTime From, DateTime To)
    : IQuery<TrackingReportResponse>;

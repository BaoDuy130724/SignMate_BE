using MediatR;
using SignMate.Application.DTOs.StudentTracking;

namespace SignMate.Application.Features.Tracking.Queries.GenerateReport;

/// <summary>
/// Handler tạo báo cáo định kỳ trung tâm. Hiện trả về khung phản hồi (URL rỗng) — chỗ này dành sẵn
/// để tích hợp xuất file PDF/Excel ở giai đoạn sau, giữ nguyên hợp đồng API với app.
/// </summary>
public class GenerateTrackingReportQueryHandler
    : IRequestHandler<GenerateTrackingReportQuery, TrackingReportResponse>
{
    /// <inheritdoc />
    public Task<TrackingReportResponse> Handle(
        GenerateTrackingReportQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TrackingReportResponse
        {
            ReportUrl = string.Empty,
            GeneratedAt = DateTime.UtcNow
        });
    }
}

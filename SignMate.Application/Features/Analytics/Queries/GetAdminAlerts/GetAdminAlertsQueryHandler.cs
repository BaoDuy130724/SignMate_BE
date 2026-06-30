using System.Globalization;
using MediatR;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;

namespace SignMate.Application.Features.Analytics.Queries.GetAdminAlerts;

/// <summary>
/// Suy ra cảnh báo từ số liệu thật bằng các ngưỡng cố định (rule-based). Không dùng AI ở đây —
/// AI chỉ phụ trách phần diễn giải ở card insight; cảnh báo cần chính xác & ổn định.
/// </summary>
public class GetAdminAlertsQueryHandler : IRequestHandler<GetAdminAlertsQuery, List<AdminAlertDto>>
{
    private readonly IMediator _mediator;

    public GetAdminAlertsQueryHandler(IMediator mediator) => _mediator = mediator;

    public async Task<List<AdminAlertDto>> Handle(GetAdminAlertsQuery request, CancellationToken cancellationToken)
    {
        var ci = CultureInfo.InvariantCulture;
        var d = await _mediator.Send(new GetGlobalAnalyticsQuery(), cancellationToken);
        var alerts = new List<AdminAlertDto>();

        // 1) Lượt luyện tập đang giảm so với chu kỳ trước.
        if (d.SessionGrowthPercent <= -30)
            alerts.Add(new AdminAlertDto
            {
                Severity = "critical",
                Title = "Lượt luyện tập giảm mạnh",
                Detail = "Số phiên luyện tập 30 ngày gần nhất giảm sâu so với 30 ngày liền trước. Cần xem lại trải nghiệm/giữ chân người dùng.",
                Metric = $"{d.SessionGrowthPercent.ToString(ci)}%"
            });
        else if (d.SessionGrowthPercent <= -15)
            alerts.Add(new AdminAlertDto
            {
                Severity = "warning",
                Title = "Lượt luyện tập đang giảm",
                Detail = "Hoạt động luyện tập có xu hướng giảm so với chu kỳ trước.",
                Metric = $"{d.SessionGrowthPercent.ToString(ci)}%"
            });

        // 2) Chất lượng học: độ chính xác trung bình thấp.
        if (d.TotalPracticeSessions > 0 && d.AverageAccuracy < 50)
            alerts.Add(new AdminAlertDto
            {
                Severity = "warning",
                Title = "Độ chính xác trung bình thấp",
                Detail = "Độ chính xác luyện tập trung bình dưới 50% — học viên có thể đang gặp khó hoặc nội dung quá khó.",
                Metric = $"{d.AverageAccuracy.ToString(ci)}%"
            });

        // 3) Không có người dùng hoạt động trong 30 ngày (dù đã có user).
        if (d.TotalUsers > 0 && d.ActiveUsersLast30Days == 0)
            alerts.Add(new AdminAlertDto
            {
                Severity = "critical",
                Title = "Không có người dùng hoạt động",
                Detail = "Đã có tài khoản nhưng không ai luyện tập trong 30 ngày qua.",
                Metric = "0 hoạt động/30 ngày"
            });

        // 4) Hôm nay chưa có hoạt động nào (thông tin).
        if (d.SessionsToday == 0 && d.AttemptsToday == 0)
            alerts.Add(new AdminAlertDto
            {
                Severity = "info",
                Title = "Hôm nay chưa có hoạt động",
                Detail = "Chưa ghi nhận phiên luyện tập nào trong hôm nay (giờ Việt Nam).",
                Metric = null
            });

        return alerts;
    }
}

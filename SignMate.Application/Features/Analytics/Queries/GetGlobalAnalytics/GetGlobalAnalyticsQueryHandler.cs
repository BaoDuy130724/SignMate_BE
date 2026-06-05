using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;

/// <summary>
/// Handler dựng dữ liệu biểu đồ cho trang phân tích quản trị: tăng trưởng người dùng theo ngày,
/// phân bổ nguồn người dùng và xếp hạng khóa học theo số lượt ghi danh.
/// </summary>
public class GetGlobalAnalyticsQueryHandler : IRequestHandler<GetGlobalAnalyticsQuery, GlobalAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetGlobalAnalyticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<GlobalAnalyticsDto> Handle(GetGlobalAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var totalUsers = await _unitOfWork.Repository<User>().Query()
            .CountAsync(cancellationToken);
        var totalCenters = await _unitOfWork.Repository<Domain.Entities.Center>().Query()
            .CountAsync(cancellationToken);
        var b2bUsers = await _unitOfWork.Repository<User>().Query()
            .CountAsync(u => u.CenterId != null, cancellationToken);

        // Thống kê hoạt động: tổng phiên luyện tập và số lượt đạt ngưỡng thành công (>= 0.8).
        var totalSessions = await _unitOfWork.Repository<PracticeSession>().Query()
            .CountAsync(cancellationToken);
        var totalSuccess = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .CountAsync(a => a.OverallScore >= 0.8f, cancellationToken);

        // Tăng trưởng 30 ngày: gom nhóm theo ngày tạo (group-by thực hiện in-memory sau khi
        // chỉ kéo về cột CreatedAt của bản ghi trong khoảng thời gian).
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var userDates = await _unitOfWork.Repository<User>().Query()
            .Where(u => u.CreatedAt >= thirtyDaysAgo)
            .Select(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        var growth = userDates
            .GroupBy(d => d.Date)
            .Select(g => new TimeSeriesDataDto { Label = g.Key.ToString("yyyy-MM-dd"), Value = g.Count() })
            .OrderBy(x => x.Label)
            .ToList();

        // Phân bổ nguồn người dùng: B2B (gắn trung tâm) so với B2C (cá nhân).
        var distribution = new List<PieChartDataDto>
        {
            new PieChartDataDto { Name = "B2B (Hợp tác)", Value = b2bUsers },
            new PieChartDataDto { Name = "B2C (Cá nhân)", Value = totalUsers - b2bUsers }
        };

        // Top 5 khóa học theo số lượt ghi danh.
        var topCourses = await _unitOfWork.Repository<Course>().Query()
            .OrderByDescending(c => c.Enrollments.Count)
            .Take(5)
            .Select(c => new BarChartDataDto { Name = c.Title, Value = c.Enrollments.Count })
            .ToListAsync(cancellationToken);

        return new GlobalAnalyticsDto
        {
            TotalUsers = totalUsers,
            TotalCenters = totalCenters,
            TotalPracticeSessions = totalSessions,
            TotalSuccessfulAttempts = totalSuccess,
            UserGrowth = growth,
            UserDistribution = distribution,
            TopCourses = topCourses
        };
    }
}

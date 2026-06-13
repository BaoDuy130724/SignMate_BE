using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Analytics.Queries.GetGlobalAnalytics;

/// <summary>
/// Handler dựng dữ liệu cho trang phân tích quản trị. Tất cả số liệu đều tính từ dữ liệu thật trong
/// DB (không hardcode/random): tăng trưởng & người dùng hoạt động, độ chính xác trung bình, hoạt động
/// trong ngày, phân bổ nguồn người dùng và xếp hạng khóa học kèm tỉ lệ hoàn thành thật.
/// </summary>
public class GetGlobalAnalyticsQueryHandler : IRequestHandler<GetGlobalAnalyticsQuery, GlobalAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetGlobalAnalyticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<GlobalAnalyticsDto> Handle(GetGlobalAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var sixtyDaysAgo = now.AddDays(-60);
        // "Hôm nay" theo giờ VN (UTC+7): mốc 00:00 giờ VN quy về UTC để so với cột thời gian (lưu UTC).
        var todayStartUtc = now.AddHours(7).Date.AddHours(-7);

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

        // Độ chính xác trung bình thật: trung bình OverallScore của mọi lượt thử (0..1) → %.
        var avgScore = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .Select(a => (double?)a.OverallScore)
            .AverageAsync(cancellationToken) ?? 0;
        var averageAccuracy = Math.Round(avgScore * 100, 1);

        // Người dùng hoạt động (có phiên luyện tập) trong 30 ngày.
        var activeUsersLast30Days = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= thirtyDaysAgo)
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Tăng trưởng phiên: 30 ngày gần nhất so với 30 ngày liền trước đó.
        var sessionsLast30 = await _unitOfWork.Repository<PracticeSession>().Query()
            .CountAsync(ps => ps.StartedAt >= thirtyDaysAgo, cancellationToken);
        var sessionsPrev30 = await _unitOfWork.Repository<PracticeSession>().Query()
            .CountAsync(ps => ps.StartedAt >= sixtyDaysAgo && ps.StartedAt < thirtyDaysAgo, cancellationToken);
        var sessionGrowthPercent = sessionsPrev30 > 0
            ? Math.Round((double)(sessionsLast30 - sessionsPrev30) / sessionsPrev30 * 100, 1)
            : (sessionsLast30 > 0 ? 100.0 : 0.0);

        // Hoạt động trong ngày (giờ VN).
        var sessionsToday = await _unitOfWork.Repository<PracticeSession>().Query()
            .CountAsync(ps => ps.StartedAt >= todayStartUtc, cancellationToken);
        var attemptsToday = await _unitOfWork.Repository<PracticeAttempt>().Query()
            .CountAsync(a => a.RecordedAt >= todayStartUtc, cancellationToken);
        var activeUsersToday = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(ps => ps.StartedAt >= todayStartUtc)
            .Select(ps => ps.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Tăng trưởng 30 ngày: gom nhóm theo ngày tạo (group-by in-memory sau khi chỉ kéo cột CreatedAt).
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

        // Top 5 khóa học theo số lượt ghi danh, kèm tỉ lệ hoàn thành thật & học viên mới 30 ngày.
        var topCourses = await _unitOfWork.Repository<Course>().Query()
            .OrderByDescending(c => c.Enrollments.Count)
            .Take(5)
            .Select(c => new CourseAnalyticsDto
            {
                Name = c.Title,
                Enrollments = c.Enrollments.Count,
                CompletionRate = c.Enrollments.Count == 0
                    ? 0
                    : Math.Round((double)c.Enrollments.Count(e => e.CompletedAt != null) / c.Enrollments.Count * 100, 1),
                NewEnrollmentsLast30Days = c.Enrollments.Count(e => e.EnrolledAt >= thirtyDaysAgo)
            })
            .ToListAsync(cancellationToken);

        return new GlobalAnalyticsDto
        {
            TotalUsers = totalUsers,
            TotalCenters = totalCenters,
            TotalPracticeSessions = totalSessions,
            TotalSuccessfulAttempts = totalSuccess,
            AverageAccuracy = averageAccuracy,
            B2BUsers = b2bUsers,
            ActiveUsersLast30Days = activeUsersLast30Days,
            SessionGrowthPercent = sessionGrowthPercent,
            SessionsToday = sessionsToday,
            AttemptsToday = attemptsToday,
            ActiveUsersToday = activeUsersToday,
            UserGrowth = growth,
            UserDistribution = distribution,
            TopCourses = topCourses
        };
    }
}

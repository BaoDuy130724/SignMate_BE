using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Analytics;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Services;

public class AnalyticsService(ISignMateDbContext db) : IAnalyticsService
{
    public async Task<GlobalAnalyticsDto> GetGlobalAnalyticsAsync()
    {
        var totalUsers = await db.Users.CountAsync();
        var totalCenters = await db.Centers.CountAsync();
        var b2bUsers = await db.Users.CountAsync(u => u.CenterId != null);
        
        // Activity stats
        var totalSessions = await db.PracticeSessions.CountAsync();
        var totalSuccess = await db.PracticeAttempts.CountAsync(a => a.OverallScore >= 0.8f);
        
        // Growth (Last 30 Days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var userDates = await db.Users
            .Where(u => u.CreatedAt >= thirtyDaysAgo)
            .Select(u => u.CreatedAt)
            .ToListAsync();

        var growth = userDates
            .GroupBy(d => d.Date)
            .Select(g => new TimeSeriesDataDto { Label = g.Key.ToString("yyyy-MM-dd"), Value = g.Count() })
            .OrderBy(x => x.Label)
            .ToList();
            
        // Distribution
        var distribution = new List<PieChartDataDto>
        {
            new PieChartDataDto { Name = "B2B (Hợp tác)", Value = b2bUsers },
            new PieChartDataDto { Name = "B2C (Cá nhân)", Value = totalUsers - b2bUsers }
        };
        
        // Top Courses
        var topCourses = await db.Courses
            .OrderByDescending(c => c.Enrollments.Count)
            .Take(5)
            .Select(c => new BarChartDataDto { Name = c.Title, Value = c.Enrollments.Count })
            .ToListAsync();

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

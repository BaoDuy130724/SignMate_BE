using System.Text.Json.Serialization;

namespace SignMate.Application.DTOs.Analytics;

public class GlobalAnalyticsDto
{
    public int TotalUsers { get; set; }
    public int TotalCenters { get; set; }
    public int TotalPracticeSessions { get; set; }
    public int TotalSuccessfulAttempts { get; set; }

    /// <summary>Độ chính xác trung bình thật (avg OverallScore của mọi lượt thử, đã *100).</summary>
    public double AverageAccuracy { get; set; }

    /// <summary>Số học viên B2B (gắn trung tâm) — để hiển thị mạng lưới đối tác.</summary>
    [JsonPropertyName("b2bUsers")]
    public int B2BUsers { get; set; }

    /// <summary>Số người dùng có ít nhất một phiên luyện tập trong 30 ngày gần nhất.</summary>
    public int ActiveUsersLast30Days { get; set; }

    /// <summary>% thay đổi số phiên luyện tập 30 ngày gần nhất so với 30 ngày liền trước.</summary>
    public double SessionGrowthPercent { get; set; }

    // ── Hoạt động trong ngày (theo giờ VN) ──
    public int SessionsToday { get; set; }
    /// <summary>Số lượt AI nhận diện (PracticeAttempt) hôm nay.</summary>
    public int AttemptsToday { get; set; }
    public int ActiveUsersToday { get; set; }

    public List<TimeSeriesDataDto> UserGrowth { get; set; } = [];
    public List<PieChartDataDto> UserDistribution { get; set; } = []; // B2B vs B2C
    public List<CourseAnalyticsDto> TopCourses { get; set; } = [];
}

public class TimeSeriesDataDto
{
    public string Label { get; set; } = null!; // "2024-03-20"
    public int Value { get; set; }
}

public class PieChartDataDto
{
    public string Name { get; set; } = null!; // "B2B"
    public int Value { get; set; }
}

public class CourseAnalyticsDto
{
    public string Name { get; set; } = null!;
    public int Enrollments { get; set; }
    /// <summary>% học viên đã hoàn thành khóa (Enrollment.CompletedAt != null).</summary>
    public double CompletionRate { get; set; }
    /// <summary>Số học viên ghi danh mới trong 30 ngày gần nhất.</summary>
    public int NewEnrollmentsLast30Days { get; set; }
}

/// <summary>
/// Nhận định do AI (Gemini) sinh ra từ số liệu thật — dùng cho card "AI Insights" / "AI phân tích doanh thu".
/// AI chỉ diễn giải số đã tính sẵn, không tự bịa dữ liệu. Degrade: <see cref="AiAvailable"/> = false khi
/// chưa cấu hình key hoặc gọi lỗi (web hiện thông báo thay vì báo lỗi).
/// </summary>
public class AdminInsightDto
{
    public string Summary { get; set; } = "";
    public List<string> Positives { get; set; } = [];
    public List<string> Concerns { get; set; } = [];
    public List<string> Recommendations { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
    public string Model { get; set; } = "";
    public bool AiAvailable { get; set; }
}

/// <summary>
/// Một cảnh báo bất thường được phát hiện bằng LUẬT (deterministic) từ số liệu thật — không để AI bịa.
/// </summary>
public class AdminAlertDto
{
    /// <summary>"info" | "warning" | "critical".</summary>
    public string Severity { get; set; } = "info";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    /// <summary>Chỉ số liên quan (vd "-18%", "47%") — tùy chọn.</summary>
    public string? Metric { get; set; }
}

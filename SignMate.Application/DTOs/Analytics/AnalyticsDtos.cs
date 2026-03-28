namespace SignMate.Application.DTOs.Analytics;

public class GlobalAnalyticsDto
{
    public int TotalUsers { get; set; }
    public int TotalCenters { get; set; }
    public int TotalPracticeSessions { get; set; }
    public int TotalSuccessfulAttempts { get; set; }
    public List<TimeSeriesDataDto> UserGrowth { get; set; } = [];
    public List<PieChartDataDto> UserDistribution { get; set; } = []; // B2B vs B2C
    public List<BarChartDataDto> TopCourses { get; set; } = [];
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

public class BarChartDataDto
{
    public string Name { get; set; } = null!; // "Căn bản"
    public int Value { get; set; }
}

using System.Text.Json.Serialization;

namespace SignMate.Application.DTOs.Admin;

public class SystemDashboardDto
{
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveCenters { get; set; }
    public double ConversionRate { get; set; } // Free -> Paid %
    public int PremiumUsers { get; set; }
    public int BasicUsers { get; set; }
    public int FreeUsers { get; set; }

    // JsonPropertyName bắt buộc: camelCase của "B2CUsers"/"B2BUsers" là "b2CUsers"/"b2BUsers",
    // nhưng Flutter đọc "b2cUsers"/"b2bUsers" — phải pin tên JSON tường minh.
    [JsonPropertyName("b2cUsers")]
    public int B2CUsers { get; set; }

    [JsonPropertyName("b2bUsers")]
    public int B2BUsers { get; set; }

    public double RetentionRate { get; set; }
}

namespace SignMate.Application.DTOs.Admin;

public class SystemDashboardDto
{
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveCenters { get; set; }
    public double ConversionRate { get; set; } // Free -> Paid %
    public int PremiumUsers { get; set; }
    public double RetentionRate { get; set; }
}

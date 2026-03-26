using System.Text.Json;
using SignMate.Domain.Entities;

namespace SignMate.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(SignMateDbContext context)
    {
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Miễn phí",
                PriceVnd = 0,
                DurationDays = 30, // Default duration for non-expiring/monthly cycle
                Type = PlanType.Free,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "5 bài học mỗi ngày", 
                    "Phản hồi đúng sai cơ bản", 
                    "Không phân tích thống kê", 
                    "Không sửa lỗi chi tiết" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Cơ bản",
                PriceVnd = 49000,
                DurationDays = 30,
                Type = PlanType.Basic,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "Bài học không giới hạn", 
                    "Phản hồi lỗi cơ bản", 
                    "Theo dõi tiến độ học tập", 
                    "Duy trì chuỗi ngày học (Streak)" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Nâng cao (Pro)",
                PriceVnd = 99000,
                DurationDays = 30,
                Type = PlanType.Pro,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "AI sửa lỗi chi tiết", 
                    "Phân tích góc độ, hình dáng tay", 
                    "Phát hiện điểm yếu cần cải thiện", 
                    "Phân tích chuyên sâu (Advanced analytics)" 
                })
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Gói Trung tâm (B2B)",
                PriceVnd = 79000, // Per learner
                DurationDays = 30,
                Type = PlanType.B2B,
                FeaturesJson = JsonSerializer.Serialize(new[] 
                { 
                    "Trang quản trị cho giáo viên", 
                    "Quản lý danh sách lớp học", 
                    "Theo dõi tiến độ học viên", 
                    "Báo cáo kết quả học tập",
                    "Yêu cầu tối thiểu 20 học viên"
                })
            }
        };

        if (!context.SubscriptionPlans.Any())
        {
            await context.SubscriptionPlans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
        }
        else
        {
            // Sync all existing plans to match the defined plans
            foreach (var newPlan in plans)
            {
                var existing = context.SubscriptionPlans.FirstOrDefault(p => p.Type == newPlan.Type);
                if (existing != null)
                {
                    existing.Name = newPlan.Name;
                    existing.PriceVnd = newPlan.PriceVnd;
                    existing.FeaturesJson = newPlan.FeaturesJson;
                }
                else
                {
                    context.SubscriptionPlans.Add(newPlan);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}

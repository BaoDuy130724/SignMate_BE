using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.API.Services;

/// <summary>
/// Background Service tự động dọn dẹp các UserSubscription (Pending) quá 24h.
/// Giúp DB không bị rác khi người dùng click tạo nhiều mã thanh toán nhưng không chuyển khoản.
/// </summary>
public class PendingSubscriptionCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingSubscriptionCleanupService> _logger;

    public PendingSubscriptionCleanupService(IServiceProvider serviceProvider, ILogger<PendingSubscriptionCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PendingSubscriptionCleanupService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Tính toán thời gian đến 2:00 AM tiếp theo (giờ địa phương của server)
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0); // 2:00 AM hôm nay
            
            if (now >= nextRun)
            {
                nextRun = nextRun.AddDays(1); // 2:00 AM ngày mai
            }

            var delay = nextRun - now;
            _logger.LogInformation("Lần dọn dẹp tiếp theo sẽ diễn ra lúc: {NextRun} (chờ {DelayHours:F2} tiếng).", nextRun, delay.TotalHours);

            try
            {
                // Chờ đến đúng 2:00 AM
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break; // Dừng ngay nếu ứng dụng bị tắt
            }

            try
            {
                _logger.LogInformation("Bắt đầu dọn dẹp các giao dịch quá hạn...");
                await CleanupPendingSubscriptionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình dọn dẹp giao dịch quá hạn.");
            }
        }

        _logger.LogInformation("PendingSubscriptionCleanupService is stopping.");
    }

    private async Task CleanupPendingSubscriptionsAsync(CancellationToken cancellationToken)
    {
        // Vì BackgroundService là Singleton, ta cần tạo Scope để resolve IUnitOfWork (Scoped)
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Lấy thời điểm cách đây 24 tiếng
        var cutoffTime = DateTime.UtcNow.AddHours(-24);

        // Tìm tất cả subscription chưa thanh toán (IsActive = false) và tạo trước cutoffTime
        var repo = unitOfWork.Repository<UserSubscription>();
        var oldPendings = repo.Query(s => !s.IsActive && s.StartDate < cutoffTime).ToList();

        if (oldPendings.Any())
        {
            foreach (var old in oldPendings)
            {
                repo.Delete(old);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Đã dọn dẹp {Count} pending subscriptions cũ hơn 24 giờ.", oldPendings.Count);
        }
    }
}

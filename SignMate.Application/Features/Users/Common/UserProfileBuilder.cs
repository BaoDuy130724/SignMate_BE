using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Common;

/// <summary>
/// Helper dựng <see cref="UserProfileDto"/> đầy đủ (gói cước, streak, XP, level, số bài hoàn thành,
/// độ chính xác luyện tập) từ một entity <see cref="User"/>.
/// Tách riêng để hai use-case xem hồ sơ và cập nhật hồ sơ tái sử dụng cùng một logic tổng hợp
/// (DRY), tránh lệch dữ liệu giữa hai luồng.
/// </summary>
internal static class UserProfileBuilder
{
    private const int XpPointsPerLevel = 500;

    /// <summary>
    /// Tổng hợp các chỉ số phụ trợ của hồ sơ và trả về DTO hoàn chỉnh cho client mobile.
    /// </summary>
    public static async Task<UserProfileDto> BuildAsync(
        IUnitOfWork unitOfWork, User user, CancellationToken cancellationToken)
    {
        // Gói cước đang hiệu lực (nếu không có thì mặc định Free).
        var activeSubscription = await unitOfWork.Repository<UserSubscription>().Query()
            .Include(us => us.Plan)
            .Where(us => us.UserId == user.Id && us.IsActive && us.EndDate >= DateTime.UtcNow)
            .OrderByDescending(us => us.EndDate)
            .FirstOrDefaultAsync(cancellationToken);
        var currentPlan = activeSubscription?.Plan.Type.ToString() ?? "Free";

        var currentStreak = await unitOfWork.Repository<Streak>().Query()
            .Where(s => s.UserId == user.Id)
            .Select(s => (int?)s.CurrentStreak)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var completedLessons = await unitOfWork.Repository<LessonProgress>().Query()
            .CountAsync(p => p.UserId == user.Id && p.Status == LessonStatus.Completed, cancellationToken);

        double avgAccuracy = await unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.UserId == user.Id)
            .Select(a => (double?)a.OverallScore)
            .AverageAsync(cancellationToken) ?? 0;

        var level = (user.XpPoints / XpPointsPerLevel) + 1;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role.ToString(),
            Plan = currentPlan,
            CenterId = user.CenterId,
            CreatedAt = user.CreatedAt,
            Streak = currentStreak,
            TotalXp = user.XpPoints,
            Level = level,
            LessonsCompleted = completedLessons,
            PracticeAccuracy = (int)Math.Round(avgAccuracy * 100)
        };
    }
}

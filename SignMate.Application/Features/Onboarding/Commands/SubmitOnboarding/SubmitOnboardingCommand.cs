using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Onboarding;

namespace SignMate.Application.Features.Onboarding.Commands.SubmitOnboarding;

/// <summary>
/// Lệnh lưu cấu hình cá nhân hóa lộ trình học (mục tiêu + trình độ) mà học viên chọn ở
/// bước onboarding — <c>POST /api/onboarding</c>. Đánh dấu user đã hoàn tất onboarding.
/// </summary>
/// <param name="UserId">Id học viên lấy từ JWT (không nhận từ body để tránh giả mạo).</param>
/// <param name="Request">Mục tiêu học tập và trình độ tự đánh giá.</param>
public record SubmitOnboardingCommand(int UserId, OnboardingRequest Request)
    : ICommand<OnboardingResponse>;

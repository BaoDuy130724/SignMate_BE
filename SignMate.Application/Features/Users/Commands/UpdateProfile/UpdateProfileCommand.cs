using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Lệnh cập nhật thông tin hồ sơ cơ bản (tên hiển thị, ảnh đại diện) của người dùng hiện tại —
/// <c>PUT /api/users/me</c>.
/// </summary>
/// <param name="UserId">Id người dùng lấy từ JWT.</param>
/// <param name="Request">Các trường cần cập nhật (chỉ áp dụng trường khác null).</param>
public record UpdateProfileCommand(int UserId, UpdateProfileRequest Request) : ICommand<UserProfileDto>;

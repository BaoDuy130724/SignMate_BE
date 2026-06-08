using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Lệnh tạo người dùng mới do SuperAdmin thực hiện (bỏ qua OTP) — <c>POST /api/users</c>.
/// </summary>
/// <param name="Request">Thông tin tài khoản cần tạo.</param>
public record CreateUserCommand(CreateUserRequest Request) : ICommand<UserProfileDto>;

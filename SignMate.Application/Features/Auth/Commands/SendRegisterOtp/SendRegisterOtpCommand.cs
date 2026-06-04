using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Features.Auth.Commands.SendRegisterOtp;

/// <summary>
/// Lệnh gửi mã OTP xác thực email khi đăng ký tài khoản — <c>POST /api/auth/send-register-otp</c>.
/// </summary>
public record SendRegisterOtpCommand(SendOtpRequest Request) : ICommand<Unit>;

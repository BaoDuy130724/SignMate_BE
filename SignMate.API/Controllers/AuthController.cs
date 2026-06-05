using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Features.Auth.Commands.ChangePassword;
using SignMate.Application.Features.Auth.Commands.ForgotPassword;
using SignMate.Application.Features.Auth.Commands.Login;
using SignMate.Application.Features.Auth.Commands.Logout;
using SignMate.Application.Features.Auth.Commands.Refresh;
using SignMate.Application.Features.Auth.Commands.Register;
using SignMate.Application.Features.Auth.Commands.ResetPassword;
using SignMate.Application.Features.Auth.Commands.SendRegisterOtp;

namespace SignMate.API.Controllers;

/// <summary>
/// Xác thực &amp; quản lý tài khoản: đăng ký (OTP), đăng nhập, làm mới token, đăng xuất,
/// quên/đặt lại/đổi mật khẩu.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    /// <summary>Gửi OTP xác thực email khi đăng ký. <c>POST /api/auth/send-register-otp</c>.</summary>
    [EnableRateLimiting("otp")]
    [HttpPost("send-register-otp")]
    public async Task<IActionResult> SendRegisterOtp([FromBody] SendOtpRequest request)
    {
        await Mediator.Send(new SendRegisterOtpCommand(request));
        return Success("Đã gửi mã OTP tới email của bạn.");
    }

    /// <summary>Tạo tài khoản mới sau khi xác thực OTP. <c>POST /api/auth/register</c>.</summary>
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await Mediator.Send(new RegisterCommand(request));
        return Success(result, "Đăng ký thành công.");
    }

    /// <summary>Đăng nhập bằng email + mật khẩu. <c>POST /api/auth/login</c>.</summary>
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await Mediator.Send(new LoginCommand(request));
        return Success(result, "Đăng nhập thành công.");
    }

    /// <summary>Làm mới phiên đăng nhập. <c>POST /api/auth/refresh</c>.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await Mediator.Send(new RefreshCommand(request.RefreshToken));
        return Success(result, "Làm mới token thành công.");
    }

    /// <summary>Đăng xuất, thu hồi refresh token. <c>POST /api/auth/logout</c>.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new LogoutCommand(userId, request.RefreshToken));
        return Success("Đăng xuất thành công.");
    }

    /// <summary>Yêu cầu OTP khôi phục mật khẩu. <c>POST /api/auth/forgot-password</c>.</summary>
    [EnableRateLimiting("otp")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await Mediator.Send(new ForgotPasswordCommand(request));
        return Success("Nếu email tồn tại, mã khôi phục đã được gửi.");
    }

    /// <summary>Đặt lại mật khẩu bằng OTP. <c>POST /api/auth/reset-password</c>.</summary>
    [EnableRateLimiting("auth")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await Mediator.Send(new ResetPasswordCommand(request));
        return Success("Đặt lại mật khẩu thành công.");
    }

    /// <summary>Đổi mật khẩu khi đã đăng nhập. <c>POST /api/auth/change-password</c>.</summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new ChangePasswordCommand(userId, request));
        return Success("Đổi mật khẩu thành công.");
    }
}

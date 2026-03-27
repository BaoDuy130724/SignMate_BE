using SignMate.Application.DTOs.Auth;

namespace SignMate.Application.Interfaces;

public interface IAuthService
{
    Task SendRegisterOtpAsync(SendOtpRequest request);
    Task<TokenResponse> RegisterAsync(RegisterRequest request);
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId, string refreshToken);
    
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

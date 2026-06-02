using MediatR;
using SignMate.Application.DTOs.Auth;
using SignMate.Application.Interfaces;

namespace SignMate.Application.Features.Auth;

public record LoginCommand(LoginRequest Request) : IRequest<TokenResponse>;
public record RegisterCommand(RegisterRequest Request) : IRequest<TokenResponse>;
public record SendRegisterOtpCommand(SendOtpRequest Request) : IRequest<Unit>;
public record RefreshCommand(string RefreshToken) : IRequest<TokenResponse>;
public record LogoutCommand(Guid UserId, string RefreshToken) : IRequest<Unit>;
public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<Unit>;
public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<Unit>;
public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<Unit>;

public class AuthCQRSHandlers : 
    IRequestHandler<LoginCommand, TokenResponse>,
    IRequestHandler<RegisterCommand, TokenResponse>,
    IRequestHandler<SendRegisterOtpCommand, Unit>,
    IRequestHandler<RefreshCommand, TokenResponse>,
    IRequestHandler<LogoutCommand, Unit>,
    IRequestHandler<ForgotPasswordCommand, Unit>,
    IRequestHandler<ResetPasswordCommand, Unit>,
    IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IAuthService _authService;

    public AuthCQRSHandlers(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Request);
    }

    public async Task<TokenResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.Request);
    }

    public async Task<Unit> Handle(SendRegisterOtpCommand request, CancellationToken cancellationToken)
    {
        await _authService.SendRegisterOtpAsync(request.Request);
        return Unit.Value;
    }

    public async Task<TokenResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshAsync(request.RefreshToken);
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.UserId, request.RefreshToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request.Request);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request.Request);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ChangePasswordAsync(request.UserId, request.Request);
        return Unit.Value;
    }
}

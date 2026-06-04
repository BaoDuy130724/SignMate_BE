using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Handler cho <see cref="ResetPasswordCommand"/>: xác thực OTP rồi băm và lưu mật khẩu mới,
/// đồng thời xóa các dấu vết token reset cũ. Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Yêu cầu không hợp lệ.");

        if (!_otpService.ValidateOtp(request.Email, "ResetPassword", request.Token))
            throw new UnauthorizedAccessException("Mã OTP không đúng hoặc đã hết hạn.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

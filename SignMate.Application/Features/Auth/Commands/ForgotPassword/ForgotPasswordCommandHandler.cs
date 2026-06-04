using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Handler cho <see cref="ForgotPasswordCommand"/>: chỉ gửi OTP khi email tồn tại, nhưng luôn
/// kết thúc êm (không tiết lộ email nào có trong hệ thống) để chống dò email — controller trả
/// thông điệp trung lập trong mọi trường hợp.
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var userExists = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => u.Email == command.Request.Email, cancellationToken);

        if (userExists)
            await _otpService.GenerateAndSendOtpAsync(command.Request.Email, "ResetPassword");

        return Unit.Value;
    }
}

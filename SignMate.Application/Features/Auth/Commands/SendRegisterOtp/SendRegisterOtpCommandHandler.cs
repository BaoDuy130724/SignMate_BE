using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.SendRegisterOtp;

/// <summary>
/// Handler cho <see cref="SendRegisterOtpCommand"/>: chặn email đã tồn tại rồi sinh &amp; gửi OTP.
/// Đây là thao tác phía hạ tầng (cache + gửi mail), không ghi DB.
/// </summary>
public class SendRegisterOtpCommandHandler : IRequestHandler<SendRegisterOtpCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public SendRegisterOtpCommandHandler(IUnitOfWork unitOfWork, IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(SendRegisterOtpCommand command, CancellationToken cancellationToken)
    {
        var emailExists = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => u.Email == command.Request.Email, cancellationToken);
        if (emailExists)
            throw new ConflictException("Email đã được sử dụng.");

        await _otpService.GenerateAndSendOtpAsync(command.Request.Email, "Register");
        return Unit.Value;
    }
}

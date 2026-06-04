using MediatR;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Handler cho <see cref="ChangePasswordCommand"/>: xác minh mật khẩu hiện tại (BCrypt) trước khi
/// băm và lưu mật khẩu mới. Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(command.UserId)
            ?? throw new UnauthorizedAccessException("Không tìm thấy người dùng.");

        if (!BCrypt.Net.BCrypt.Verify(command.Request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

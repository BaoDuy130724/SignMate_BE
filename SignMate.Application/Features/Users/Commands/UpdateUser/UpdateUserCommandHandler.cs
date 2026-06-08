using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Handler cho <see cref="UpdateUserCommand"/>: cập nhật một phần hồ sơ/quyền của người dùng rồi trả
/// về hồ sơ đầy đủ. Chỉ ghi đè trường được gửi lên (khác null) — giữ partial update nhất quán.
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(User), command.Id);

        var request = command.Request;
        if (request.FullName is not null)
            user.FullName = request.FullName;
        if (request.AvatarUrl is not null)
            user.AvatarUrl = request.AvatarUrl;
        if (request.Role is not null && Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            user.Role = role;
        // CenterId == 0 là quy ước "gỡ khỏi trung tâm" → set null.
        if (request.CenterId is not null)
            user.CenterId = request.CenterId == 0 ? null : request.CenterId;
        user.UpdatedAt = DateTime.UtcNow;

        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, user, cancellationToken);
    }
}

using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Handler cho <see cref="UpdateProfileCommand"/>: cập nhật một phần (partial update) các trường
/// hồ sơ rồi trả về hồ sơ đầy đủ sau khi lưu. Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        // Partial update: chỉ ghi đè trường được gửi lên (khác null).
        if (command.Request.FullName is not null)
            user.FullName = command.Request.FullName;
        if (command.Request.AvatarUrl is not null)
            user.AvatarUrl = command.Request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, user, cancellationToken);
    }
}

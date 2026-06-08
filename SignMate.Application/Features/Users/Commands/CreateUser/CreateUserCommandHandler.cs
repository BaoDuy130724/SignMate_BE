using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler cho <see cref="CreateUserCommand"/>: tạo tài khoản (đã băm mật khẩu) kèm bản ghi
/// <see cref="Streak"/> khởi tạo. Khác luồng đăng ký công khai ở chỗ không yêu cầu OTP và cho phép
/// gán vai trò/trung tâm tùy ý — chỉ SuperAdmin được gọi.
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var emailExists = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (emailExists)
            throw new ConflictException("Email đã được sử dụng.");

        // Vai trò đã được validator đảm bảo hợp lệ nên parse an toàn ở đây.
        Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = role,
            CenterId = request.CenterId,
            IsOnboarded = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<User>().AddAsync(user);

        // Khởi tạo streak rỗng để các luồng đọc streak không gặp null (dùng navigation cho EF gán FK).
        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            User = user,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, user, cancellationToken);
    }
}

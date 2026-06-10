using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Features.Subscription.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Commands.CreateCenterUser;

/// <summary>
/// Handler cho <see cref="CreateCenterUserCommand"/>: kiểm tra trung tâm tồn tại và email chưa dùng,
/// rồi tạo tài khoản kèm bản ghi Streak khởi tạo. User và Streak được ghi trong cùng một
/// <c>SaveChangesAsync</c> (EF gói một transaction) nên đảm bảo Atomicity.
/// </summary>
public class CreateCenterUserCommandHandler : IRequestHandler<CreateCenterUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCenterUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(CreateCenterUserCommand command, CancellationToken cancellationToken)
    {
        var centerExists = await _unitOfWork.Repository<CenterEntity>().Query()
            .AnyAsync(c => c.Id == command.CenterId, cancellationToken);
        if (!centerExists)
            throw new NotFoundException("Trung tâm không tồn tại.");

        var emailTaken = await _unitOfWork.Repository<User>().Query()
            .AnyAsync(u => u.Email == command.Request.Email, cancellationToken);
        if (emailTaken)
            throw new ConflictException("Email đã được sử dụng.");

        var user = new User
        {
            Email = command.Request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Request.Password),
            FullName = command.Request.FullName,
            Role = command.Role,
            CenterId = command.CenterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<User>().AddAsync(user);

        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            User = user,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        // Học viên thuộc trung tâm → tự động gán gói B2B.
        if (command.Role == UserRole.Student)
            await SubscriptionActivation.AutoAssignB2BAsync(_unitOfWork, user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

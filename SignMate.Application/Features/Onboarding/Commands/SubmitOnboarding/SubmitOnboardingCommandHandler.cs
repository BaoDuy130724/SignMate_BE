using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Onboarding.Commands.SubmitOnboarding;

/// <summary>
/// Handler cho <see cref="SubmitOnboardingCommand"/>: cập nhật mục tiêu/trình độ và bật cờ
/// <c>IsOnboarded</c> cho học viên. Đây là thao tác ghi đơn trên một entity nên một lần
/// <c>SaveChangesAsync</c> đã đảm bảo tính atomic (không cần transaction tường minh).
/// </summary>
public class SubmitOnboardingCommandHandler : IRequestHandler<SubmitOnboardingCommand, OnboardingResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOnboardingCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<OnboardingResponse> Handle(SubmitOnboardingCommand command, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(command.UserId)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        user.Goal = command.Request.Goal;
        user.Level = command.Request.Level;
        user.IsOnboarded = true;

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OnboardingResponse { UserId = user.Id, Success = true };
    }
}

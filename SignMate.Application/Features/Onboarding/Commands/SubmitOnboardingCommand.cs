using FluentValidation;
using MediatR;
using SignMate.Application.DTOs.Common;
using SignMate.Application.DTOs.Onboarding;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Onboarding.Commands;

public record SubmitOnboardingCommand(int UserId, OnboardingRequest Request) : IRequest<ApiResponse<OnboardingResponse>>;

public class SubmitOnboardingCommandValidator : AbstractValidator<SubmitOnboardingCommand>
{
    public SubmitOnboardingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Request).NotNull().WithMessage("Request payload is required.");
        RuleFor(x => x.Request.Goal).NotEmpty().WithMessage("Goal is required.");
        RuleFor(x => x.Request.Level).NotEmpty().WithMessage("Level is required.");
    }
}

public class SubmitOnboardingCommandHandler : IRequestHandler<SubmitOnboardingCommand, ApiResponse<OnboardingResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOnboardingCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ApiResponse<OnboardingResponse>> Handle(SubmitOnboardingCommand command, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(command.UserId);
            if (user == null)
            {
                return ApiResponse<OnboardingResponse>.FailureResult("User not found.");
            }

            user.Goal = command.Request.Goal;
            user.Level = command.Request.Level;
            user.IsOnboarded = true;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var data = new OnboardingResponse { UserId = command.UserId, Success = true };
            return ApiResponse<OnboardingResponse>.SuccessResult(data, "Onboarding submitted successfully.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

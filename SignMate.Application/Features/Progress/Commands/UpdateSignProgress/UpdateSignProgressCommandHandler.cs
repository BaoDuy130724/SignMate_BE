using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Progress.Commands.UpdateSignProgress;

/// <summary>
/// Handler cho <see cref="UpdateSignProgressCommand"/>: tạo mới hoặc cập nhật tiến độ luyện ký hiệu,
/// tăng số lần thử và cập nhật cờ thành thạo. Ghi đơn một aggregate nên một lần <c>SaveChanges</c>
/// đã đủ tính nguyên tử.
/// </summary>
public class UpdateSignProgressCommandHandler : IRequestHandler<UpdateSignProgressCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSignProgressCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(UpdateSignProgressCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        _ = await _unitOfWork.Repository<Sign>().GetByIdAsync(request.SignId)
            ?? throw new NotFoundException(nameof(Sign), request.SignId);

        var progress = await _unitOfWork.Repository<SignProgress>().Query()
            .FirstOrDefaultAsync(sp => sp.UserId == command.UserId && sp.SignId == request.SignId, cancellationToken);

        if (progress is null)
        {
            progress = new SignProgress
            {
                UserId = command.UserId,
                SignId = request.SignId,
                IsMastered = request.IsMastered,
                AttemptCount = 1,
                LastPracticedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<SignProgress>().AddAsync(progress);
        }
        else
        {
            progress.IsMastered = request.IsMastered;
            progress.AttemptCount++;
            progress.LastPracticedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

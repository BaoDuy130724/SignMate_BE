using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Commands.EndSession;

/// <summary>
/// Handler cho <see cref="EndSessionCommand"/>: đánh dấu thời điểm kết thúc cho phiên luyện tập
/// thuộc đúng người dùng. Ghi đơn nên một lần <c>SaveChanges</c> là đủ nguyên tử.
/// </summary>
public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public EndSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(EndSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await _unitOfWork.Repository<PracticeSession>().Query()
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && s.UserId == command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(PracticeSession), command.SessionId);

        if (!session.EndedAt.HasValue)
        {
            session.EndedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}

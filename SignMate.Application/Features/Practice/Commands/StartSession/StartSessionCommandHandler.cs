using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Practice;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Practice.Commands.StartSession;

/// <summary>
/// Handler cho <see cref="StartSessionCommand"/>: xác thực ký hiệu tồn tại rồi tạo bản ghi
/// phiên luyện tập mới (chưa có lượt thử) và trả về Id phiên cho client theo dõi.
/// </summary>
public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, StartSessionResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public StartSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<StartSessionResponse> Handle(StartSessionCommand command, CancellationToken cancellationToken)
    {
        _ = await _unitOfWork.Repository<Sign>().GetByIdAsync(command.SignId)
            ?? throw new NotFoundException(nameof(Sign), command.SignId);

        var session = new PracticeSession
        {
            UserId = command.UserId,
            SignId = command.SignId,
            StartedAt = DateTime.UtcNow,
            TotalAttempts = 0
        };

        await _unitOfWork.Repository<PracticeSession>().AddAsync(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StartSessionResponse { SessionId = session.Id };
    }
}

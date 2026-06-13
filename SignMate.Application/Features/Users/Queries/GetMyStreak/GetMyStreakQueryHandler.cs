using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Streaks.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Queries.GetMyStreak;

/// <summary>
/// Handler đọc streak của người dùng. Nếu chưa có bản ghi (user mới), trả về streak 0
/// với ngày hoạt động là hôm nay để client luôn có dữ liệu hiển thị an toàn.
/// Chuỗi hiện tại trả về là chuỗi hiệu lực (0 nếu đã đứt quãng), không phải giá trị thô trong DB.
/// </summary>
public class GetMyStreakQueryHandler : IRequestHandler<GetMyStreakQuery, StreakDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyStreakQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<StreakDto> Handle(GetMyStreakQuery query, CancellationToken cancellationToken)
    {
        var streak = await _unitOfWork.Repository<Streak>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == query.UserId, cancellationToken);

        return streak is null
            ? new StreakDto
            {
                CurrentStreak = 0,
                LongestStreak = 0,
                LastActiveDate = StreakActivity.TodayVietnam()
            }
            : new StreakDto
            {
                CurrentStreak = StreakActivity.EffectiveCurrent(streak),
                LongestStreak = streak.LongestStreak,
                LastActiveDate = streak.LastActiveDate
            };
    }
}

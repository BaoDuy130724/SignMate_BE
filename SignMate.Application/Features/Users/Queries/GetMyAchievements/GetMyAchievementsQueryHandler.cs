using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Queries.GetMyAchievements;

/// <summary>
/// Handler đọc danh sách thành tích của người dùng, projection trực tiếp sang DTO.
/// </summary>
public class GetMyAchievementsQueryHandler : IRequestHandler<GetMyAchievementsQuery, List<AchievementDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyAchievementsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<AchievementDto>> Handle(GetMyAchievementsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<UserAchievement>().Query()
            .AsNoTracking()
            .Where(ua => ua.UserId == query.UserId)
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new AchievementDto
            {
                Id = ua.Achievement.Id,
                Name = ua.Achievement.Name,
                Description = ua.Achievement.Description,
                IconUrl = ua.Achievement.IconUrl,
                EarnedAt = ua.EarnedAt
            })
            .ToListAsync(cancellationToken);
    }
}

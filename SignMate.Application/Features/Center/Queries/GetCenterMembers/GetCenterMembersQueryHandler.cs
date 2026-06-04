using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Queries.GetCenterMembers;

/// <summary>
/// Handler liệt kê thành viên của trung tâm theo vai trò, trả về hồ sơ cơ bản.
/// </summary>
public class GetCenterMembersQueryHandler : IRequestHandler<GetCenterMembersQuery, List<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCenterMembersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<UserProfileDto>> Handle(GetCenterMembersQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<User>().Query()
            .AsNoTracking()
            .Where(u => u.CenterId == query.CenterId && u.Role == query.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                Role = u.Role.ToString(),
                CenterId = u.CenterId,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}

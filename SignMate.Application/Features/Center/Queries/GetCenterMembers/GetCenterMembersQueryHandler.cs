using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
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
    private readonly ICurrentUser _currentUser;

    public GetCenterMembersQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<UserProfileDto>> Handle(GetCenterMembersQuery query, CancellationToken cancellationToken)
    {
        // Phân quyền multi-tenant (IDOR/BOLA): CenterAdmin chỉ được truy cập dữ liệu trung tâm của chính mình.
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != query.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập thông tin thành viên của trung tâm khác.");
        }
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

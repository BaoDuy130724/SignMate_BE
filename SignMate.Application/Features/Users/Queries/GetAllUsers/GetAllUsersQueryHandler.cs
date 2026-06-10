using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Handler liệt kê người dùng cho trang quản trị. Trả về thông tin hồ sơ cơ bản (không kèm
/// các chỉ số học tập nặng) để danh sách tải nhanh.
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<UserProfileDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = _unitOfWork.Repository<User>().Query().AsNoTracking();

        // CenterAdmin chỉ thấy user trong center của mình.
        if (query.CallerCenterId.HasValue)
            usersQuery = usersQuery.Where(u => u.CenterId == query.CallerCenterId.Value);

        // Lọc theo vai trò nếu client truyền giá trị hợp lệ.
        if (!string.IsNullOrWhiteSpace(query.Role)
            && Enum.TryParse<UserRole>(query.Role, ignoreCase: true, out var parsedRole))
        {
            usersQuery = usersQuery.Where(u => u.Role == parsedRole);
        }

        return await usersQuery
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

using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Queries.GetMyProfile;

/// <summary>
/// Handler nạp hồ sơ người dùng hiện tại và tổng hợp các chỉ số học tập qua <see cref="UserProfileBuilder"/>.
/// </summary>
public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMyProfileQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(query.UserId)
            ?? throw new NotFoundException(nameof(User), query.UserId);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, user, cancellationToken);
    }
}

using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Handler cho <see cref="GetUserByIdQuery"/>: trả về hồ sơ đầy đủ (gói cước, streak, XP…) của
/// người dùng được chỉ định, ném 404 nếu không tồn tại.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(query.Id)
            ?? throw new NotFoundException(nameof(User), query.Id);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, user, cancellationToken);
    }
}

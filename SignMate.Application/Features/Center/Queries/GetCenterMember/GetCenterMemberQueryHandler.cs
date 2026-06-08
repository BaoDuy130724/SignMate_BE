using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Queries.GetCenterMember;

/// <summary>
/// Handler cho <see cref="GetCenterMemberQuery"/>: trả về thông tin chi tiết thành viên nếu thuộc cùng trung tâm.
/// </summary>
public class GetCenterMemberQueryHandler : IRequestHandler<GetCenterMemberQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCenterMemberQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(GetCenterMemberQuery query, CancellationToken cancellationToken)
    {
        var centerAdmin = await _unitOfWork.Repository<User>().GetByIdAsync(query.CurrentUserId)
            ?? throw new NotFoundException(nameof(User), query.CurrentUserId);

        if (centerAdmin.CenterId != query.CenterId)
            throw new BadRequestException("Bạn không có quyền truy cập trung tâm này.");

        var member = await _unitOfWork.Repository<User>().GetByIdAsync(query.UserId);
        if (member == null || member.CenterId != query.CenterId)
            throw new NotFoundException("Thành viên không tồn tại trong trung tâm này.");

        return await UserProfileBuilder.BuildAsync(_unitOfWork, member, cancellationToken);
    }
}

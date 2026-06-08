using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.User;
using SignMate.Application.Features.Users.Common;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Commands.UpdateCenterMember;

/// <summary>
/// Handler cho <see cref="UpdateCenterMemberCommand"/>: cập nhật họ tên của thành viên trung tâm.
/// </summary>
public class UpdateCenterMemberCommandHandler : IRequestHandler<UpdateCenterMemberCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCenterMemberCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<UserProfileDto> Handle(UpdateCenterMemberCommand command, CancellationToken cancellationToken)
    {
        var centerAdmin = await _unitOfWork.Repository<User>().GetByIdAsync(command.CurrentUserId)
            ?? throw new NotFoundException(nameof(User), command.CurrentUserId);

        if (centerAdmin.CenterId != command.CenterId)
            throw new BadRequestException("Bạn không có quyền truy cập trung tâm này.");

        var member = await _unitOfWork.Repository<User>().GetByIdAsync(command.UserId);
        if (member == null || member.CenterId != command.CenterId)
            throw new NotFoundException("Thành viên không tồn tại trong trung tâm này.");

        member.FullName = command.Request.FullName;
        member.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await UserProfileBuilder.BuildAsync(_unitOfWork, member, cancellationToken);
    }
}

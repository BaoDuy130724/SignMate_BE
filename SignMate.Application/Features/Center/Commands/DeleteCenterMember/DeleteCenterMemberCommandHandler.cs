using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Commands.DeleteCenterMember;

/// <summary>
/// Handler cho <see cref="DeleteCenterMemberCommand"/>: gỡ/xóa thành viên khỏi trung tâm, kiểm tra ràng buộc FK.
/// </summary>
public class DeleteCenterMemberCommandHandler : IRequestHandler<DeleteCenterMemberCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCenterMemberCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCenterMemberCommand command, CancellationToken cancellationToken)
    {
        if (command.UserId == command.CurrentUserId)
            throw new BadRequestException("Bạn không thể xóa chính tài khoản đang đăng nhập.");

        var centerAdmin = await _unitOfWork.Repository<User>().GetByIdAsync(command.CurrentUserId)
            ?? throw new NotFoundException(nameof(User), command.CurrentUserId);

        if (centerAdmin.CenterId != command.CenterId)
            throw new BadRequestException("Bạn không có quyền truy cập trung tâm này.");

        var member = await _unitOfWork.Repository<User>().GetByIdAsync(command.UserId);
        if (member == null || member.CenterId != command.CenterId)
            throw new NotFoundException("Thành viên không tồn tại trong trung tâm này.");

        if (member.Role == UserRole.Teacher)
        {
            var isTeaching = await _unitOfWork.Repository<Class>().Query()
                .AnyAsync(c => c.TeacherId == command.UserId && c.CenterId == command.CenterId, cancellationToken);
            if (isTeaching)
                throw new BadRequestException("Không thể xóa giáo viên vì đang phụ trách ít nhất một lớp học.");
        }
        else if (member.Role == UserRole.Student)
        {
            var isInClass = await _unitOfWork.Repository<ClassStudent>().Query()
                .AnyAsync(cs => cs.StudentId == command.UserId && cs.Class.CenterId == command.CenterId, cancellationToken);
            if (isInClass)
                throw new BadRequestException("Không thể xóa học viên vì đang tham gia ít nhất một lớp học.");
        }

        _unitOfWork.Repository<User>().Delete(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

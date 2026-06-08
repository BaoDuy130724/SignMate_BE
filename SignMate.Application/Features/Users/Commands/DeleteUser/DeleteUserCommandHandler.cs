using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Users.Commands.DeleteUser;

/// <summary>
/// Handler cho <see cref="DeleteUserCommand"/>: xóa người dùng nếu tồn tại. Chặn SuperAdmin tự xóa
/// tài khoản đang đăng nhập để tránh khóa mình khỏi hệ thống.
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == command.CurrentUserId)
            throw new BadRequestException("Bạn không thể xóa chính tài khoản đang đăng nhập.");

        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(command.Id)
            ?? throw new NotFoundException(nameof(User), command.Id);

        repo.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

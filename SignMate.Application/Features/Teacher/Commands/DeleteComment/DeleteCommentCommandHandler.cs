using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Commands.DeleteComment;

/// <summary>
/// Handler cho <see cref="DeleteCommentCommand"/>: xóa nhận xét nếu đúng giáo viên sở hữu.
/// </summary>
public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<TeacherComment>().GetByIdAsync(command.CommentId)
            ?? throw new NotFoundException(nameof(TeacherComment), command.CommentId);

        if (comment.TeacherId != command.TeacherId)
            throw new ForbiddenException("Bạn không có quyền xóa nhận xét của giáo viên khác.");

        _unitOfWork.Repository<TeacherComment>().Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

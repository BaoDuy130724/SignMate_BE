using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Commands.UpdateComment;

/// <summary>
/// Handler cho <see cref="UpdateCommentCommand"/>: cập nhật nội dung nhận xét nếu đúng giáo viên sở hữu.
/// </summary>
public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, TeacherCommentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<TeacherCommentDto> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<TeacherComment>().GetByIdAsync(command.CommentId)
            ?? throw new NotFoundException(nameof(TeacherComment), command.CommentId);

        if (comment.TeacherId != command.TeacherId)
            throw new ForbiddenException("Bạn không có quyền sửa nhận xét của giáo viên khác.");

        comment.Content = command.Request.Content;
        _unitOfWork.Repository<TeacherComment>().Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(command.TeacherId)
            ?? throw new NotFoundException(nameof(User), command.TeacherId);

        return new TeacherCommentDto
        {
            Id = comment.Id,
            TeacherId = comment.TeacherId,
            TeacherName = teacher.FullName,
            StudentId = comment.StudentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}

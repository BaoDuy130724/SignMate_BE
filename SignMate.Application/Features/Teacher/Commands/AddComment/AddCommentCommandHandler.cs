using MediatR;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Commands.AddComment;

/// <summary>
/// Handler cho <see cref="AddCommentCommand"/>: lưu nhận xét của giáo viên cho học viên.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, TeacherCommentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<TeacherCommentDto> Handle(AddCommentCommand command, CancellationToken cancellationToken)
    {
        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(command.TeacherId)
            ?? throw new NotFoundException(nameof(User), command.TeacherId);

        var comment = new TeacherComment
        {
            TeacherId = command.TeacherId,
            StudentId = command.Request.StudentId,
            Content = command.Request.Content,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<TeacherComment>().AddAsync(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TeacherCommentDto
        {
            Id = comment.Id,
            TeacherId = command.TeacherId,
            TeacherName = teacher.FullName,
            StudentId = comment.StudentId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}

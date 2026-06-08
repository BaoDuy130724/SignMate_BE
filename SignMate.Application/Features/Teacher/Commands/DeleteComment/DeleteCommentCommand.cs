using MediatR;
using SignMate.Application.Common.Messaging;

namespace SignMate.Application.Features.Teacher.Commands.DeleteComment;

/// <summary>
/// Lệnh xóa nhận xét cá nhân hóa của giáo viên cho học viên — <c>DELETE /api/teacher/comments/{id}</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên thực hiện xóa.</param>
/// <param name="CommentId">Id nhận xét cần xóa.</param>
public record DeleteCommentCommand(int TeacherId, int CommentId) : ICommand<Unit>;

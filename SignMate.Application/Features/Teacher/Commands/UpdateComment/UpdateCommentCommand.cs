using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Features.Teacher.Commands.UpdateComment;

/// <summary>
/// Lệnh sửa nhận xét cá nhân hóa của giáo viên cho học viên — <c>PUT /api/teacher/comments/{id}</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên thực hiện chỉnh sửa.</param>
/// <param name="CommentId">Id nhận xét cần sửa.</param>
/// <param name="Request">Nội dung nhận xét mới.</param>
public record UpdateCommentCommand(int TeacherId, int CommentId, UpdateCommentRequest Request) 
    : ICommand<TeacherCommentDto>;

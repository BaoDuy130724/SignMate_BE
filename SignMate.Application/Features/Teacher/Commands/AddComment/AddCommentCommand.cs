using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Features.Teacher.Commands.AddComment;

/// <summary>
/// Lệnh giáo viên gửi nhận xét cá nhân hóa cho một học viên — <c>POST /api/teacher/comments</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên lấy từ JWT.</param>
/// <param name="Request">Học viên nhận và nội dung nhận xét.</param>
public record AddCommentCommand(int TeacherId, CreateCommentRequest Request) : ICommand<TeacherCommentDto>;

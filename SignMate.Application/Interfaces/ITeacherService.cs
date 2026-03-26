using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Interfaces;

public interface ITeacherService
{
    Task<TeacherCommentDto> AddCommentAsync(Guid teacherId, CreateCommentRequest request);
    Task<List<TeacherCommentDto>> GetStudentCommentsAsync(Guid studentId);
}

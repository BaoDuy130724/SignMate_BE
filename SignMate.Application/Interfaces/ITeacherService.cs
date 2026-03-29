using SignMate.Application.DTOs.Class;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Interfaces;

public interface ITeacherService
{
    Task<TeacherCommentDto> AddCommentAsync(Guid teacherId, CreateCommentRequest request);
    Task<List<TeacherCommentDto>> GetStudentCommentsAsync(Guid studentId);
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid teacherId);
    Task<List<ClassDto>> GetTeacherClassesAsync(Guid teacherId);
    Task<List<ClassStudentDto>> GetTeacherStudentsAsync(Guid teacherId);
}

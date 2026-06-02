using SignMate.Application.DTOs.Class;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Interfaces;

public interface ITeacherService
{
    Task<TeacherCommentDto> AddCommentAsync(int teacherId, CreateCommentRequest request);
    Task<List<TeacherCommentDto>> GetStudentCommentsAsync(int studentId);
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(int teacherId);
    Task<List<ClassDto>> GetTeacherClassesAsync(int teacherId);
    Task<List<ClassStudentDto>> GetTeacherStudentsAsync(int teacherId);
}

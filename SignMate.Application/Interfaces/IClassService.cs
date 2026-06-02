using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Interfaces;

public interface IClassService
{
    Task<List<ClassDto>> GetClassesAsync(int centerId);
    Task<ClassDto> CreateClassAsync(int centerId, CreateClassRequest request);
    Task AddStudentsAsync(int classId, AddStudentsRequest request);
    Task<List<ClassStudentDto>> GetClassStudentsAsync(int classId);
    Task AssignLessonAsync(int classId, int teacherId, AssignLessonRequest request);
}

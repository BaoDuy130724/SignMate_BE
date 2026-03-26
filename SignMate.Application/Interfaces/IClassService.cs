using SignMate.Application.DTOs.Class;

namespace SignMate.Application.Interfaces;

public interface IClassService
{
    Task<List<ClassDto>> GetClassesAsync(Guid centerId);
    Task<ClassDto> CreateClassAsync(Guid centerId, CreateClassRequest request);
    Task AddStudentsAsync(Guid classId, AddStudentsRequest request);
    Task<List<ClassStudentDto>> GetClassStudentsAsync(Guid classId);
    Task AssignLessonAsync(Guid classId, Guid teacherId, AssignLessonRequest request);
}

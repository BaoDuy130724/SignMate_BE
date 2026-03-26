using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Interfaces;

public interface ICourseService
{
    Task<List<CourseDto>> GetCoursesAsync(string? search, string? level);
    Task<CourseDetailDto?> GetCourseByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, Guid createdBy);
    Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseRequest request);
    Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId);
    Task<LessonDetailDto?> GetLessonByIdAsync(Guid lessonId);
    Task<bool> SetSignReferenceAsync(SetReferenceRequest request);
}

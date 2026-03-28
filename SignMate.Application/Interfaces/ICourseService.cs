using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Interfaces;

public interface ICourseService
{
    Task<List<CourseDto>> GetCoursesAsync(string? search, string? level, bool includeUnpublished = false);
    Task<CourseDetailDto?> GetCourseByIdAsync(Guid id);
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, Guid createdBy);
    Task<CourseDto?> UpdateCourseAsync(Guid id, UpdateCourseRequest request);
    Task DeleteCourseAsync(Guid id);
    Task<List<LessonDto>> GetLessonsByCourseAsync(Guid courseId);
    Task<LessonDetailDto?> GetLessonByIdAsync(Guid lessonId);
    Task<LessonDto> CreateLessonAsync(Guid courseId, CreateLessonRequest request);
    Task<LessonDto?> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request);
    Task DeleteLessonAsync(Guid lessonId);
    Task<bool> SetSignReferenceAsync(SetReferenceRequest request);
}

using SignMate.Application.DTOs.Course;

namespace SignMate.Application.Interfaces;

public interface ICourseService
{
    Task<List<CourseDto>> GetCoursesAsync(string? search, string? level, bool includeUnpublished = false);
    Task<CourseDetailDto?> GetCourseByIdAsync(int id);
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, int createdBy);
    Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseRequest request);
    Task DeleteCourseAsync(int id);
    Task<List<LessonDto>> GetLessonsByCourseAsync(int courseId);
    Task<LessonDetailDto?> GetLessonByIdAsync(int lessonId);
    Task<LessonDto> CreateLessonAsync(int courseId, CreateLessonRequest request);
    Task<LessonDto?> UpdateLessonAsync(int lessonId, UpdateLessonRequest request);
    Task DeleteLessonAsync(int lessonId);
    Task<bool> SetSignReferenceAsync(SetReferenceRequest request);
}

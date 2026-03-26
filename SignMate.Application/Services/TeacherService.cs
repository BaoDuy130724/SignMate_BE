using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly ISignMateDbContext _db;

    public TeacherService(ISignMateDbContext db) => _db = db;

    public async Task<TeacherCommentDto> AddCommentAsync(Guid teacherId, CreateCommentRequest request)
    {
        var comment = new TeacherComment
        {
            Id = Guid.NewGuid(), TeacherId = teacherId, StudentId = request.StudentId,
            Content = request.Content, CreatedAt = DateTime.UtcNow
        };
        _db.TeacherComments.Add(comment);
        await _db.SaveChangesAsync();

        var teacher = await _db.Users.FindAsync(teacherId);

        return new TeacherCommentDto
        {
            Id = comment.Id, TeacherId = teacherId, TeacherName = teacher?.FullName ?? "",
            StudentId = request.StudentId, Content = request.Content, CreatedAt = comment.CreatedAt
        };
    }

    public async Task<List<TeacherCommentDto>> GetStudentCommentsAsync(Guid studentId)
    {
        return await _db.TeacherComments
            .Where(tc => tc.StudentId == studentId)
            .Select(tc => new TeacherCommentDto
            {
                Id = tc.Id, TeacherId = tc.TeacherId, TeacherName = tc.Teacher.FullName,
                StudentId = tc.StudentId, Content = tc.Content, CreatedAt = tc.CreatedAt
            }).ToListAsync();
    }
}

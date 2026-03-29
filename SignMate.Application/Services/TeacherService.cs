using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
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

    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid teacherId)
    {
        var teacher = await _db.Users.FindAsync(teacherId);
        if (teacher == null) throw new InvalidOperationException("Teacher not found");
        
        var totalClasses = await _db.Classes.CountAsync(c => c.TeacherId == teacherId);
        var totalStudents = await _db.ClassStudents
            .Include(cs => cs.Class)
            .Where(cs => cs.Class.TeacherId == teacherId)
            .Select(cs => cs.StudentId)
            .Distinct()
            .CountAsync();

        return new TeacherDashboardDto
        {
            TotalClasses = totalClasses,
            TotalStudents = totalStudents
        };
    }

    public async Task<List<ClassDto>> GetTeacherClassesAsync(Guid teacherId)
    {
        return await _db.Classes
            .Where(c => c.TeacherId == teacherId)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FullName,
                StudentCount = c.ClassStudents.Count
            })
            .ToListAsync();
    }

    public async Task<List<ClassStudentDto>> GetTeacherStudentsAsync(Guid teacherId)
    {
        return await _db.ClassStudents
            .Include(cs => cs.Class)
            .Where(cs => cs.Class.TeacherId == teacherId)
            .Select(cs => new ClassStudentDto
            {
                StudentId = cs.StudentId,
                FullName = cs.Student.FullName,
                Email = cs.Student.Email
            })
            .Distinct()
            .ToListAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class ClassService : IClassService
{
    private readonly ISignMateDbContext _db;

    public ClassService(ISignMateDbContext db) => _db = db;

    public async Task<List<ClassDto>> GetClassesAsync(Guid centerId)
    {
        return await _db.Classes
            .Where(c => c.CenterId == centerId)
            .Select(c => new ClassDto
            {
                Id = c.Id, Name = c.Name, TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FullName, StudentCount = c.ClassStudents.Count
            }).ToListAsync();
    }

    public async Task<ClassDto> CreateClassAsync(Guid centerId, CreateClassRequest request)
    {
        var c = new Class
        {
            Id = Guid.NewGuid(), CenterId = centerId,
            Name = request.Name, TeacherId = request.TeacherId, CreatedAt = DateTime.UtcNow
        };
        _db.Classes.Add(c);
        await _db.SaveChangesAsync();
        return new ClassDto { Id = c.Id, Name = c.Name, TeacherId = c.TeacherId };
    }

    public async Task AddStudentsAsync(Guid classId, AddStudentsRequest request)
    {
        foreach (var sid in request.StudentIds)
        {
            if (!await _db.ClassStudents.AnyAsync(cs => cs.ClassId == classId && cs.StudentId == sid))
            {
                _db.ClassStudents.Add(new ClassStudent { ClassId = classId, StudentId = sid });
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<ClassStudentDto>> GetClassStudentsAsync(Guid classId)
    {
        return await _db.ClassStudents
            .Where(cs => cs.ClassId == classId)
            .Select(cs => new ClassStudentDto
            {
                StudentId = cs.StudentId, FullName = cs.Student.FullName, Email = cs.Student.Email
            }).ToListAsync();
    }

    public async Task AssignLessonAsync(Guid classId, Guid teacherId, AssignLessonRequest request)
    {
        _db.LessonAssignments.Add(new LessonAssignment
        {
            Id = Guid.NewGuid(), ClassId = classId, LessonId = request.LessonId,
            AssignedBy = teacherId, AssignedAt = DateTime.UtcNow, DueDate = request.DueDate
        });
        await _db.SaveChangesAsync();
    }
}

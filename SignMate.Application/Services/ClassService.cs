using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class ClassService : IClassService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClassService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<ClassDto>> GetClassesAsync(int centerId)
    {
        return await _unitOfWork.Repository<Class>().Query()
            .Where(c => c.CenterId == centerId)
            .Select(c => new ClassDto
            {
                Id = c.Id, Name = c.Name, TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FullName, StudentCount = c.ClassStudents.Count,
                Students = c.ClassStudents.Select(cs => new ClassStudentDetailDto
                {
                    Id = cs.Student.Id,
                    FullName = cs.Student.FullName,
                    Email = cs.Student.Email,
                    PracticeAccuracy = cs.Student.PracticeSessions
                        .SelectMany(ps => ps.Attempts)
                        .Any()
                        ? (int)(cs.Student.PracticeSessions
                            .SelectMany(ps => ps.Attempts)
                            .Average(a => a.OverallScore) * 100)
                        : 0
                }).ToList()
            }).ToListAsync();
    }

    public async Task<ClassDto> CreateClassAsync(int centerId, CreateClassRequest request)
    {
        var c = new Class
        {
            Id = 0, CenterId = centerId,
            Name = request.Name, TeacherId = request.TeacherId, CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<Class>().AddAsync(c);
        await _unitOfWork.SaveChangesAsync();
        return new ClassDto { Id = c.Id, Name = c.Name, TeacherId = c.TeacherId };
    }

    public async Task AddStudentsAsync(int classId, AddStudentsRequest request)
    {
        foreach (var sid in request.StudentIds)
        {
            if (!await _unitOfWork.Repository<ClassStudent>().Query()
                .AnyAsync(cs => cs.ClassId == classId && cs.StudentId == sid))
            {
                await _unitOfWork.Repository<ClassStudent>().AddAsync(
                    new ClassStudent { ClassId = classId, StudentId = sid });
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ClassStudentDto>> GetClassStudentsAsync(int classId)
    {
        return await _unitOfWork.Repository<ClassStudent>().Query()
            .Where(cs => cs.ClassId == classId)
            .Select(cs => new ClassStudentDto
            {
                StudentId = cs.StudentId, FullName = cs.Student.FullName, Email = cs.Student.Email
            }).ToListAsync();
    }

    public async Task AssignLessonAsync(int classId, int teacherId, AssignLessonRequest request)
    {
        await _unitOfWork.Repository<LessonAssignment>().AddAsync(new LessonAssignment
        {
            Id = 0, ClassId = classId, LessonId = request.LessonId,
            AssignedBy = teacherId, AssignedAt = DateTime.UtcNow, DueDate = request.DueDate
        });
        await _unitOfWork.SaveChangesAsync();
    }
}

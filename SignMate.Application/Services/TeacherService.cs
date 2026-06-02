using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeacherService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TeacherCommentDto> AddCommentAsync(int teacherId, CreateCommentRequest request)
    {
        var comment = new TeacherComment
        {
            Id = 0, TeacherId = teacherId, StudentId = request.StudentId,
            Content = request.Content, CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<TeacherComment>().AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(teacherId);

        return new TeacherCommentDto
        {
            Id = comment.Id, TeacherId = teacherId, TeacherName = teacher?.FullName ?? "",
            StudentId = request.StudentId, Content = request.Content, CreatedAt = comment.CreatedAt
        };
    }

    public async Task<List<TeacherCommentDto>> GetStudentCommentsAsync(int studentId)
    {
        return await _unitOfWork.Repository<TeacherComment>().Query()
            .Where(tc => tc.StudentId == studentId)
            .Select(tc => new TeacherCommentDto
            {
                Id = tc.Id, TeacherId = tc.TeacherId, TeacherName = tc.Teacher.FullName,
                StudentId = tc.StudentId, Content = tc.Content, CreatedAt = tc.CreatedAt
            }).ToListAsync();
    }

    public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(int teacherId)
    {
        var teacher = await _unitOfWork.Repository<User>().GetByIdAsync(teacherId);
        if (teacher == null) throw new InvalidOperationException("Teacher not found");
        
        var totalClasses = await _unitOfWork.Repository<Class>().Query()
            .CountAsync(c => c.TeacherId == teacherId);
        var totalStudents = await _unitOfWork.Repository<ClassStudent>().Query()
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

    public async Task<List<ClassDto>> GetTeacherClassesAsync(int teacherId)
    {
        return await _unitOfWork.Repository<Class>().Query()
            .Where(c => c.TeacherId == teacherId)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                Name = c.Name,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.FullName,
                StudentCount = c.ClassStudents.Count,
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
            })
            .ToListAsync();
    }

    public async Task<List<ClassStudentDto>> GetTeacherStudentsAsync(int teacherId)
    {
        return await _unitOfWork.Repository<ClassStudent>().Query()
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

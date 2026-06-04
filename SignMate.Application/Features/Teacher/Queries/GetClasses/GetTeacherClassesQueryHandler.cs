using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Queries.GetClasses;

/// <summary>
/// Handler liệt kê lớp học của giáo viên. Độ chính xác trung bình của mỗi học viên được tính từ
/// điểm các lượt luyện tập (quy về %), trả 0 nếu học viên chưa luyện tập lần nào.
/// </summary>
public class GetTeacherClassesQueryHandler : IRequestHandler<GetTeacherClassesQuery, List<ClassDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeacherClassesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<ClassDto>> Handle(GetTeacherClassesQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Class>().Query()
            .AsNoTracking()
            .Where(c => c.TeacherId == query.TeacherId)
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
                    PracticeAccuracy = cs.Student.PracticeSessions.SelectMany(ps => ps.Attempts).Any()
                        ? (int)(cs.Student.PracticeSessions.SelectMany(ps => ps.Attempts).Average(a => a.OverallScore) * 100)
                        : 0
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}

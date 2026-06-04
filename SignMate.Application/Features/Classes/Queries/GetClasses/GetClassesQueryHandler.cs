using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Queries.GetClasses;

/// <summary>
/// Handler liệt kê lớp học theo trung tâm. Độ chính xác trung bình của mỗi học viên tính từ các lượt
/// luyện tập (quy về %), trả 0 nếu chưa luyện tập.
/// </summary>
public class GetClassesQueryHandler : IRequestHandler<GetClassesQuery, List<ClassDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClassesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<ClassDto>> Handle(GetClassesQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Class>().Query()
            .AsNoTracking()
            .Where(c => c.CenterId == query.CenterId)
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

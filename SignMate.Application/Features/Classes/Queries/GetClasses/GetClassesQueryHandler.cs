using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
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
    private readonly ICurrentUser _currentUser;

    public GetClassesQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<ClassDto>> Handle(GetClassesQuery query, CancellationToken cancellationToken)
    {
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != query.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập danh sách lớp của trung tâm khác.");
        }

        if (_currentUser.Role == UserRole.Teacher.ToString() && _currentUser.CenterId != query.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập danh sách lớp của trung tâm khác.");
        }

        var dbQuery = _unitOfWork.Repository<Class>().Query()
            .AsNoTracking()
            .Where(c => c.CenterId == query.CenterId);

        if (_currentUser.Role == UserRole.Teacher.ToString())
        {
            dbQuery = dbQuery.Where(c => c.TeacherId == _currentUser.UserId);
        }

        return await dbQuery
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

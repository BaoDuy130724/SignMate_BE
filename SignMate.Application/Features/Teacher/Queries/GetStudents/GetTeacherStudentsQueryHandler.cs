using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Queries.GetStudents;

/// <summary>
/// Handler liệt kê (không trùng) học viên thuộc các lớp của giáo viên kèm thông tin liên lạc.
/// </summary>
public class GetTeacherStudentsQueryHandler : IRequestHandler<GetTeacherStudentsQuery, List<ClassStudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeacherStudentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<ClassStudentDto>> Handle(GetTeacherStudentsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<ClassStudent>().Query()
            .AsNoTracking()
            .Where(cs => cs.Class.TeacherId == query.TeacherId
                      && cs.Student.CenterId == cs.Class.CenterId)
            .Select(cs => new ClassStudentDto
            {
                StudentId = cs.StudentId,
                FullName = cs.Student.FullName,
                Email = cs.Student.Email
            })
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}

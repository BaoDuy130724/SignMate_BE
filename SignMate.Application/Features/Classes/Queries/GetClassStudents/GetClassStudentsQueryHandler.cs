using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Class;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Queries.GetClassStudents;

/// <summary>
/// Handler liệt kê học viên thuộc một lớp kèm thông tin liên lạc.
/// </summary>
public class GetClassStudentsQueryHandler : IRequestHandler<GetClassStudentsQuery, List<ClassStudentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClassStudentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<ClassStudentDto>> Handle(GetClassStudentsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<ClassStudent>().Query()
            .AsNoTracking()
            .Where(cs => cs.ClassId == query.ClassId)
            .Select(cs => new ClassStudentDto
            {
                StudentId = cs.StudentId,
                FullName = cs.Student.FullName,
                Email = cs.Student.Email
            })
            .ToListAsync(cancellationToken);
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
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
    private readonly ICurrentUser _currentUser;

    public GetClassStudentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<List<ClassStudentDto>> Handle(GetClassStudentsQuery query, CancellationToken cancellationToken)
    {
        var cls = await _unitOfWork.Repository<Class>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.ClassId, cancellationToken);
        if (cls == null)
            throw new NotFoundException("Lớp học không tồn tại.");

        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && cls.CenterId != _currentUser.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập lớp học của trung tâm khác.");
        }

        if (_currentUser.Role == UserRole.Teacher.ToString() && cls.TeacherId != _currentUser.UserId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập lớp học do giáo viên khác phụ trách.");
        }
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

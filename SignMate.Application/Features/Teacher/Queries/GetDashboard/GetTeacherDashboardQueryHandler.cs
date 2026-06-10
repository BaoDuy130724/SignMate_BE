using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Queries.GetDashboard;

/// <summary>
/// Handler đếm số lớp giáo viên phụ trách và số học sinh duy nhất trong các lớp đó.
/// </summary>
public class GetTeacherDashboardQueryHandler : IRequestHandler<GetTeacherDashboardQuery, TeacherDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTeacherDashboardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<TeacherDashboardDto> Handle(GetTeacherDashboardQuery query, CancellationToken cancellationToken)
    {
        var totalClasses = await _unitOfWork.Repository<Class>().Query()
            .CountAsync(c => c.TeacherId == query.TeacherId, cancellationToken);

        var totalStudents = await _unitOfWork.Repository<ClassStudent>().Query()
            .Where(cs => cs.Class.TeacherId == query.TeacherId
                      && cs.Student.CenterId == cs.Class.CenterId)
            .Select(cs => cs.StudentId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new TeacherDashboardDto
        {
            TotalClasses = totalClasses,
            TotalStudents = totalStudents
        };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Teacher;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Teacher.Queries.GetStudentComments;

/// <summary>
/// Handler liệt kê nhận xét của giáo viên dành cho một học viên cụ thể.
/// </summary>
public class GetStudentCommentsQueryHandler : IRequestHandler<GetStudentCommentsQuery, List<TeacherCommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStudentCommentsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<List<TeacherCommentDto>> Handle(GetStudentCommentsQuery query, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<TeacherComment>().Query()
            .AsNoTracking()
            .Where(tc => tc.StudentId == query.StudentId)
            .OrderByDescending(tc => tc.CreatedAt)
            .Select(tc => new TeacherCommentDto
            {
                Id = tc.Id,
                TeacherId = tc.TeacherId,
                TeacherName = tc.Teacher.FullName,
                StudentId = tc.StudentId,
                Content = tc.Content,
                CreatedAt = tc.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}

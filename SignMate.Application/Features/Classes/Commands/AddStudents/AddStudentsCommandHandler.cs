using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.AddStudents;

/// <summary>
/// Handler cho <see cref="AddStudentsCommand"/>: thêm các học viên chưa có vào lớp (bỏ qua học viên
/// đã thuộc lớp để tránh trùng). Toàn bộ thêm mới nằm trong một SaveChanges nên atomic.
/// </summary>
public class AddStudentsCommandHandler : IRequestHandler<AddStudentsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddStudentsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(AddStudentsCommand command, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<ClassStudent>();

        // Lấy sẵn các học viên đã có trong lớp để lọc trùng trong một truy vấn.
        var existingStudentIds = await repo.Query()
            .Where(cs => cs.ClassId == command.ClassId && command.Request.StudentIds.Contains(cs.StudentId))
            .Select(cs => cs.StudentId)
            .ToListAsync(cancellationToken);

        var toAdd = command.Request.StudentIds.Distinct().Except(existingStudentIds);
        foreach (var studentId in toAdd)
            await repo.AddAsync(new ClassStudent { ClassId = command.ClassId, StudentId = studentId });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

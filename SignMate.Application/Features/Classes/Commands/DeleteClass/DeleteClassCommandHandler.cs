using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.DeleteClass;

/// <summary>
/// Handler cho <see cref="DeleteClassCommand"/>: xóa lớp học nếu tồn tại và không còn học viên.
/// </summary>
public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteClassCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var cls = await _unitOfWork.Repository<Class>().Query()
            .Include(c => c.ClassStudents)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId && c.CenterId == command.CenterId, cancellationToken)
            ?? throw new NotFoundException(nameof(Class), command.ClassId);

        if (cls.ClassStudents.Any())
            throw new BadRequestException("Không thể xóa lớp học vì vẫn còn học viên. Hãy gỡ tất cả học viên khỏi lớp trước.");

        _unitOfWork.Repository<Class>().Delete(cls);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

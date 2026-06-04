using MediatR;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Classes.Commands.AssignLesson;

/// <summary>
/// Handler cho <see cref="AssignLessonCommand"/>: tạo bản ghi giao bài cho lớp.
/// Ghi đơn một entity nên SaveChanges là atomic.
/// </summary>
public class AssignLessonCommandHandler : IRequestHandler<AssignLessonCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignLessonCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Unit> Handle(AssignLessonCommand command, CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<LessonAssignment>().AddAsync(new LessonAssignment
        {
            ClassId = command.ClassId,
            LessonId = command.Request.LessonId,
            AssignedBy = command.TeacherId,
            AssignedAt = DateTime.UtcNow,
            DueDate = command.Request.DueDate
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

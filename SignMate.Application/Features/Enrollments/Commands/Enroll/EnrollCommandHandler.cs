using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Enrollment;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Enrollments.Commands.Enroll;

/// <summary>
/// Handler cho <see cref="EnrollCommand"/>: kiểm tra trùng đăng ký và sự tồn tại của khóa học
/// trước khi tạo bản ghi enrollment. Ghi đơn một entity nên một lần SaveChanges là atomic.
/// </summary>
public class EnrollCommandHandler : IRequestHandler<EnrollCommand, EnrollmentResultDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<EnrollmentResultDto> Handle(EnrollCommand command, CancellationToken cancellationToken)
    {
        var courseId = command.Request.CourseId;

        // Chống đăng ký trùng (idempotency ở mức nghiệp vụ).
        var alreadyEnrolled = await _unitOfWork.Repository<Enrollment>().Query()
            .AnyAsync(e => e.UserId == command.UserId && e.CourseId == courseId, cancellationToken);
        if (alreadyEnrolled)
            throw new ConflictException("Bạn đã đăng ký khóa học này rồi.");

        // Khóa học phải tồn tại mới cho đăng ký.
        var courseExists = await _unitOfWork.Repository<Course>().Query()
            .AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
            throw new NotFoundException(nameof(Course), courseId);

        var enrollment = new Enrollment
        {
            UserId = command.UserId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Enrollment>().AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentResultDto { EnrollmentId = enrollment.Id };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;
using CenterEntity = SignMate.Domain.Entities.Center;

namespace SignMate.Application.Features.Center.Queries.GetCenterDashboard;

/// <summary>
/// Handler tổng hợp số liệu trung tâm: sĩ số, học viên đang hoạt động, độ chính xác trung bình,
/// tổng phút luyện tập và số học viên mới trong tháng. Các phép thống kê được đẩy xuống DB tối đa.
/// </summary>
public class GetCenterDashboardQueryHandler : IRequestHandler<GetCenterDashboardQuery, CenterDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetCenterDashboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<CenterDashboardDto> Handle(GetCenterDashboardQuery query, CancellationToken cancellationToken)
    {
        // Phân quyền multi-tenant (IDOR/BOLA): CenterAdmin chỉ được xem dashboard của trung tâm mình.
        if (_currentUser.Role == UserRole.CenterAdmin.ToString() && _currentUser.CenterId != query.CenterId)
        {
            throw new ForbiddenException("Bạn không có quyền truy cập thông tin của trung tâm khác.");
        }
        var centerId = query.CenterId;
        var center = await _unitOfWork.Repository<CenterEntity>().GetByIdAsync(centerId)
            ?? throw new NotFoundException(nameof(CenterEntity), centerId);

        var studentsQuery = _unitOfWork.Repository<User>().Query()
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Student);

        var totalStudents = await studentsQuery.CountAsync(cancellationToken);
        var activeLearners = await studentsQuery.CountAsync(u => u.PracticeSessions.Any(), cancellationToken);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newStudentsThisMonth = await studentsQuery.CountAsync(u => u.CreatedAt >= monthStart, cancellationToken);

        // Độ chính xác trung bình toàn trung tâm (qua các lượt luyện tập của học viên thuộc trung tâm).
        var centerAttempts = _unitOfWork.Repository<PracticeAttempt>().Query()
            .Where(a => a.Session.User.CenterId == centerId && a.Session.User.Role == UserRole.Student);
        double avgAccuracy = (await centerAttempts
            .Select(a => (double?)a.OverallScore)
            .AverageAsync(cancellationToken) ?? 0) * 100;

        // Tổng phút luyện tập của các phiên đã kết thúc.
        var finishedSessions = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(s => s.User.CenterId == centerId && s.User.Role == UserRole.Student && s.EndedAt != null)
            .Select(s => new { s.StartedAt, s.EndedAt })
            .ToListAsync(cancellationToken);
        var totalMinutes = finishedSessions.Sum(s => (int)(s.EndedAt!.Value - s.StartedAt).TotalMinutes);

        return new CenterDashboardDto
        {
            CenterName = center.Name,
            MaxSeats = center.MaxSeats,
            TotalStudents = totalStudents,
            ActiveLearners = activeLearners,
            AverageAccuracy = Math.Round(avgAccuracy, 1),
            TotalPracticeMinutes = totalMinutes,
            NewStudentsThisMonth = newStudentsThisMonth
        };
    }
}

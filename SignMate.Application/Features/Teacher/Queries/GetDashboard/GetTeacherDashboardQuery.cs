using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Teacher;

namespace SignMate.Application.Features.Teacher.Queries.GetDashboard;

/// <summary>
/// Truy vấn số liệu tổng quan cho giáo viên: tổng lớp và tổng học sinh đang quản lý —
/// <c>GET /api/teacher/dashboard</c>.
/// </summary>
/// <param name="TeacherId">Id giáo viên lấy từ JWT.</param>
public record GetTeacherDashboardQuery(int TeacherId) : IQuery<TeacherDashboardDto>;

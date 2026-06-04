using MediatR;
using SignMate.Application.Common.Messaging;
using SignMate.Application.DTOs.Center;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Center.Commands.CreateCenterUser;

/// <summary>
/// Lệnh tạo một tài khoản người dùng thuộc trung tâm với vai trò chỉ định (CenterAdmin/Teacher/Student).
/// Gộp ba luồng tạo user của trung tâm vào một command (chỉ khác <see cref="UserRole"/>) để tránh
/// lặp logic (DRY) — phục vụ các endpoint <c>POST /api/centers/{id}/admin|teachers|students</c>.
/// </summary>
/// <param name="CenterId">Id trung tâm chứa người dùng.</param>
/// <param name="Role">Vai trò của tài khoản được tạo.</param>
/// <param name="Request">Thông tin đăng nhập và họ tên.</param>
public record CreateCenterUserCommand(int CenterId, UserRole Role, CreateCenterAdminRequest Request)
    : ICommand<Unit>;

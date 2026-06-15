using SignMate.Application.Common.Exceptions;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Features.Courses.Common;

/// <summary>
/// Phân quyền quản lý (tạo/sửa/xóa) nội dung khóa học theo phân tầng center.
/// - SuperAdmin: quản lý mọi khóa (gồm khóa global của nền tảng).
/// - Còn lại (CenterAdmin/Teacher): CHỈ quản lý khóa của chính center mình
///   (khóa global CenterId=null bị từ chối — không được đụng nội dung của nền tảng).
/// </summary>
public static class ContentAccess
{
    public static void EnsureCanManage(int? courseCenterId, ICurrentUser user)
    {
        if (user.Role == nameof(UserRole.SuperAdmin)) return;
        if (courseCenterId != null && courseCenterId == user.CenterId) return;
        throw new ForbiddenException("Bạn không có quyền chỉnh sửa nội dung này.");
    }
}

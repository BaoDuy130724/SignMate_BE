using SignMate.Application.DTOs.Center;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Interfaces;

public interface ICenterService
{
    Task<List<CenterDto>> GetCentersAsync();
    Task<CenterDto> CreateCenterAsync(CenterDto center);
    Task<CenterDashboardDto> GetCenterDashboardAsync(Guid centerId);
    Task CreateCenterAdminAsync(Guid centerId, CreateCenterAdminRequest request);
    Task<List<UserProfileDto>> GetCenterTeachersAsync(Guid centerId);
    Task CreateTeacherAsync(Guid centerId, CreateCenterAdminRequest request);
    Task<List<UserProfileDto>> GetCenterStudentsAsync(Guid centerId);
    Task CreateStudentAsync(Guid centerId, CreateCenterAdminRequest request);
}

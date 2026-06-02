using SignMate.Application.DTOs.Center;
using SignMate.Application.DTOs.User;

namespace SignMate.Application.Interfaces;

public interface ICenterService
{
    Task<List<CenterDto>> GetCentersAsync();
    Task<CenterDto> CreateCenterAsync(CenterDto center);
    Task<CenterDashboardDto> GetCenterDashboardAsync(int centerId);
    Task CreateCenterAdminAsync(int centerId, CreateCenterAdminRequest request);
    Task<List<UserProfileDto>> GetCenterTeachersAsync(int centerId);
    Task CreateTeacherAsync(int centerId, CreateCenterAdminRequest request);
    Task<List<UserProfileDto>> GetCenterStudentsAsync(int centerId);
    Task CreateStudentAsync(int centerId, CreateCenterAdminRequest request);
}

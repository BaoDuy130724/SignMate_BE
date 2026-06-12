using System.Security.Claims;
using SignMate.Application.Interfaces;

namespace SignMate.API.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int UserId
    {
        get
        {
            var claimVal = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claimVal, out var id) ? id : 0;
        }
    }

    public string Role => User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public int? CenterId
    {
        get
        {
            var claimVal = User?.FindFirst("centerId")?.Value;
            return int.TryParse(claimVal, out var id) ? id : null;
        }
    }
}

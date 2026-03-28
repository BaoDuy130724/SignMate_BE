using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/centers")]
[Authorize(Roles = "SuperAdmin,CenterAdmin")]
public class CentersController : ControllerBase
{
    private readonly ICenterService _centerService;

    public CentersController(ICenterService centerService)
        => _centerService = centerService;

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCenters()
        => Ok(await _centerService.GetCentersAsync());

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenter([FromBody] CenterDto request)
    {
        var center = await _centerService.CreateCenterAsync(request);
        return CreatedAtAction(nameof(GetCenters), new { id = center.Id }, center);
    }

    [HttpGet("{id:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid id)
        => Ok(await _centerService.GetCenterDashboardAsync(id));

    [HttpPost("{id:guid}/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenterAdmin(Guid id, [FromBody] CreateCenterAdminRequest request)
    {
        try
        {
            await _centerService.CreateCenterAdminAsync(id, request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetTeachers(Guid id)
        => Ok(await _centerService.GetCenterTeachersAsync(id));

    [HttpPost("{id:guid}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateTeacher(Guid id, [FromBody] CreateCenterAdminRequest request)
    {
        try
        {
            await _centerService.CreateTeacherAsync(id, request);
            return Ok(new { message = "Tạo giáo viên thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetStudents(Guid id)
        => Ok(await _centerService.GetCenterStudentsAsync(id));

    [HttpPost("{id:guid}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateStudent(Guid id, [FromBody] CreateCenterAdminRequest request)
    {
        try
        {
            await _centerService.CreateStudentAsync(id, request);
            return Ok(new { message = "Tạo học viên thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

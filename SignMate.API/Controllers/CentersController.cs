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

    [HttpGet("{id:int}/dashboard")]
    public async Task<IActionResult> GetDashboard(int id)
        => Ok(await _centerService.GetCenterDashboardAsync(id));

    [HttpPost("{id:int}/admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateCenterAdmin(int id, [FromBody] CreateCenterAdminRequest request)
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

    [HttpGet("{id:int}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetTeachers(int id)
        => Ok(await _centerService.GetCenterTeachersAsync(id));

    [HttpPost("{id:int}/teachers")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateTeacher(int id, [FromBody] CreateCenterAdminRequest request)
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

    [HttpGet("{id:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> GetStudents(int id)
        => Ok(await _centerService.GetCenterStudentsAsync(id));

    [HttpPost("{id:int}/students")]
    [Authorize(Roles = "CenterAdmin")]
    public async Task<IActionResult> CreateStudent(int id, [FromBody] CreateCenterAdminRequest request)
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

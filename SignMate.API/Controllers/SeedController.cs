using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Interfaces;
using SignMate.Infrastructure.Data;

namespace SignMate.API.Controllers;

/// <summary>
/// Endpoint tiện ích để nạp (seed) bộ dữ liệu chuẩn vào database — dùng khi dựng môi trường
/// dev/demo. Bao bọc <see cref="DatabaseSeeder"/> qua HTTP để khỏi phải chạy tay.
/// </summary>
[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly ISignMateDbContext _db;

    public SeedController(ISignMateDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Nạp bộ dữ liệu chuẩn. <c>POST /api/seed</c> (idempotent — chỉ bù phần thiếu).
    /// <c>POST /api/seed?reset=true</c> để <b>XÓA toàn bộ DB hiện tại</b> rồi seed lại từ đầu (tất định).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Seed([FromQuery] bool reset = false)
    {
        try
        {
            if (_db is SignMateDbContext context)
            {
                await DatabaseSeeder.SeedAsync(context, reset);
                return Ok(new
                {
                    message = reset
                        ? "Database đã được ghi đè và nạp lại bộ dữ liệu chuẩn."
                        : "Database seeded successfully!",
                    reset
                });
            }
            return StatusCode(500, "Invalid DB Context type.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Seeding failed.", error = ex.Message, detail = ex.InnerException?.Message });
        }
    }
}

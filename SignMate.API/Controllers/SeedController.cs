using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public SeedController(ISignMateDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    /// <summary>
    /// Nạp bộ dữ liệu chuẩn. <c>POST /api/seed</c> (idempotent — chỉ bù phần thiếu).
    /// <c>POST /api/seed?reset=true</c> để <b>XÓA toàn bộ DB hiện tại</b> rồi seed lại từ đầu (tất định).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Seed(
        [FromQuery] bool reset = false,
        [FromHeader(Name = "X-Seed-Key")] string? seedKey = null)
    {
        // Chặn hoàn toàn ngoài Development.
        if (!_env.IsDevelopment())
            return NotFound();

        // reset=true (xóa DB) chỉ cho phép kèm khóa bí mật.
        if (reset)
        {
            var expectedKey = _config["Seed:AdminKey"];
            if (string.IsNullOrEmpty(expectedKey) || seedKey != expectedKey)
            {
                return Unauthorized(new { message = "Thiếu hoặc sai khóa seed (X-Seed-Key)." });
            }
        }

        try
        {
            if (_db is SignMateDbContext context)
            {
                await DatabaseSeeder.SeedAsync(context, _config, _env.IsDevelopment(), reset);
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
        catch (Exception)
        {
            // Bảo mật: Ẩn chi tiết exception của DB để tránh rò rỉ cấu trúc hoặc thông tin nhạy cảm.
            return StatusCode(500, new { message = "Seeding failed." });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Interfaces;
using SignMate.Infrastructure.Data;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly ISignMateDbContext _db;

    public SeedController(ISignMateDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Seed()
    {
        try
        {
            if (_db is SignMateDbContext context)
            {
                await DatabaseSeeder.SeedAsync(context);
                return Ok(new { message = "Database seeded successfully!" });
            }
            return StatusCode(500, "Invalid DB Context type.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Seeding failed.", error = ex.Message });
        }
    }
}

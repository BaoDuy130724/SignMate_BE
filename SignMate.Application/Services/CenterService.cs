using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Center;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class CenterService : ICenterService
{
    private readonly ISignMateDbContext _db;

    public CenterService(ISignMateDbContext db) => _db = db;

    public async Task<List<CenterDto>> GetCentersAsync()
    {
        return await _db.Centers.Select(c => new CenterDto
        {
            Id = c.Id, Name = c.Name, ContactPerson = c.ContactPerson,
            Phone = c.Phone, Email = c.Email, MaxSeats = c.MaxSeats, IsActive = c.IsActive
        }).ToListAsync();
    }

    public async Task<CenterDto> CreateCenterAsync(CenterDto centerDto)
    {
        var center = new Center
        {
            Id = Guid.NewGuid(), Name = centerDto.Name, ContactPerson = centerDto.ContactPerson,
            Phone = centerDto.Phone, Email = centerDto.Email,
            MaxSeats = centerDto.MaxSeats, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _db.Centers.Add(center);
        await _db.SaveChangesAsync();
        centerDto.Id = center.Id;
        return centerDto;
    }

    public async Task<CenterDashboardDto> GetCenterDashboardAsync(Guid centerId)
    {
        var students = await _db.Users.Where(u => u.CenterId == centerId).ToListAsync();
        var studentIds = students.Select(u => u.Id).ToList();
        
        var attempts = await _db.PracticeSessions
            .Where(s => studentIds.Contains(s.UserId))
            .SelectMany(s => s.Attempts)
            .ToListAsync();
            
        double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;

        return new CenterDashboardDto
        {
            TotalStudents = students.Count,
            ActiveLearners = students.Count(u => u.PracticeSessions.Any()),
            AverageAccuracy = Math.Round(avgAcc, 1)
        };
    }
}

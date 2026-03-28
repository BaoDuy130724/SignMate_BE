using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Center;
using SignMate.Application.DTOs.User;
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
        var center = await _db.Centers.FindAsync(centerId);
        if (center == null) throw new InvalidOperationException("Center not found");

        var students = await _db.Users.Where(u => u.CenterId == centerId && u.Role == UserRole.Student).ToListAsync();
        var studentIds = students.Select(u => u.Id).ToList();
        
        var attempts = await _db.PracticeSessions
            .Where(s => studentIds.Contains(s.UserId))
            .SelectMany(s => s.Attempts)
            .ToListAsync();
            
        double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;

        return new CenterDashboardDto
        {
            CenterName = center.Name,
            MaxSeats = center.MaxSeats,
            TotalStudents = students.Count,
            ActiveLearners = students.Count(u => u.PracticeSessions.Any()),
            AverageAccuracy = Math.Round(avgAcc, 1)
        };
    }

    public async Task CreateCenterAdminAsync(Guid centerId, CreateCenterAdminRequest request)
    {
        var center = await _db.Centers.FindAsync(centerId) ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.CenterAdmin,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        
        _db.Streaks.Add(new Streak
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<UserProfileDto>> GetCenterTeachersAsync(Guid centerId)
    {
        return await _db.Users
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Teacher)
            .Select(u => new UserProfileDto
            {
                Id = u.Id, Email = u.Email, FullName = u.FullName,
                AvatarUrl = u.AvatarUrl, Role = u.Role.ToString(), CenterId = u.CenterId,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
    }

    public async Task CreateTeacherAsync(Guid centerId, CreateCenterAdminRequest request)
    {
        var center = await _db.Centers.FindAsync(centerId) ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Teacher,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        
        _db.Streaks.Add(new Streak
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<UserProfileDto>> GetCenterStudentsAsync(Guid centerId)
    {
        return await _db.Users
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Student)
            .Select(u => new UserProfileDto
            {
                Id = u.Id, Email = u.Email, FullName = u.FullName,
                AvatarUrl = u.AvatarUrl, Role = u.Role.ToString(), CenterId = u.CenterId,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
    }

    public async Task CreateStudentAsync(Guid centerId, CreateCenterAdminRequest request)
    {
        var center = await _db.Centers.FindAsync(centerId) ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Student,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        
        _db.Streaks.Add(new Streak
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _db.SaveChangesAsync();
    }
}

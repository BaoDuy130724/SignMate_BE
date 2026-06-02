using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Center;
using SignMate.Application.DTOs.User;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.Application.Services;

public class CenterService : ICenterService
{
    private readonly IUnitOfWork _unitOfWork;

    public CenterService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<List<CenterDto>> GetCentersAsync()
    {
        return await _unitOfWork.Repository<Center>().Query()
            .Select(c => new CenterDto
            {
                Id = c.Id, Name = c.Name, ContactPerson = c.ContactPerson,
                Phone = c.Phone, Email = c.Email, MaxSeats = c.MaxSeats, IsActive = c.IsActive
            }).ToListAsync();
    }

    public async Task<CenterDto> CreateCenterAsync(CenterDto centerDto)
    {
        var center = new Center
        {
            Id = 0, Name = centerDto.Name, ContactPerson = centerDto.ContactPerson,
            Phone = centerDto.Phone, Email = centerDto.Email,
            MaxSeats = centerDto.MaxSeats, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<Center>().AddAsync(center);
        await _unitOfWork.SaveChangesAsync();
        centerDto.Id = center.Id;
        return centerDto;
    }

    public async Task<CenterDashboardDto> GetCenterDashboardAsync(int centerId)
    {
        var center = await _unitOfWork.Repository<Center>().GetByIdAsync(centerId);
        if (center == null) throw new InvalidOperationException("Center not found");

        var students = await _unitOfWork.Repository<User>().Query()
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Student)
            .ToListAsync();
        var studentIds = students.Select(u => u.Id).ToList();
        
        var attempts = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(s => studentIds.Contains(s.UserId))
            .SelectMany(s => s.Attempts)
            .ToListAsync();
            
        double avgAcc = attempts.Any() ? attempts.Average(a => a.OverallScore) * 100 : 0;

        // Calculate total practice minutes from sessions
        var sessions = await _unitOfWork.Repository<PracticeSession>().Query()
            .Where(s => studentIds.Contains(s.UserId) && s.EndedAt != null)
            .ToListAsync();
        int totalMinutes = sessions.Sum(s => (int)(s.EndedAt! - s.StartedAt).Value.TotalMinutes);

        // New students this month
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        int newStudents = students.Count(u => u.CreatedAt >= monthStart);

        return new CenterDashboardDto
        {
            CenterName = center.Name,
            MaxSeats = center.MaxSeats,
            TotalStudents = students.Count,
            ActiveLearners = students.Count(u => u.PracticeSessions.Any()),
            AverageAccuracy = Math.Round(avgAcc, 1),
            TotalPracticeMinutes = totalMinutes,
            NewStudentsThisMonth = newStudents
        };
    }

    public async Task CreateCenterAdminAsync(int centerId, CreateCenterAdminRequest request)
    {
        var center = await _unitOfWork.Repository<Center>().GetByIdAsync(centerId) 
            ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _unitOfWork.Repository<User>().Query().AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = 0,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.CenterAdmin,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        
        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            Id = 0, UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<UserProfileDto>> GetCenterTeachersAsync(int centerId)
    {
        return await _unitOfWork.Repository<User>().Query()
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Teacher)
            .Select(u => new UserProfileDto
            {
                Id = u.Id, Email = u.Email, FullName = u.FullName,
                AvatarUrl = u.AvatarUrl, Role = u.Role.ToString(), CenterId = u.CenterId,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
    }

    public async Task CreateTeacherAsync(int centerId, CreateCenterAdminRequest request)
    {
        var center = await _unitOfWork.Repository<Center>().GetByIdAsync(centerId) 
            ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _unitOfWork.Repository<User>().Query().AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = 0,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Teacher,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        
        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            Id = 0, UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<UserProfileDto>> GetCenterStudentsAsync(int centerId)
    {
        return await _unitOfWork.Repository<User>().Query()
            .Where(u => u.CenterId == centerId && u.Role == UserRole.Student)
            .Select(u => new UserProfileDto
            {
                Id = u.Id, Email = u.Email, FullName = u.FullName,
                AvatarUrl = u.AvatarUrl, Role = u.Role.ToString(), CenterId = u.CenterId,
                CreatedAt = u.CreatedAt
            }).ToListAsync();
    }

    public async Task CreateStudentAsync(int centerId, CreateCenterAdminRequest request)
    {
        var center = await _unitOfWork.Repository<Center>().GetByIdAsync(centerId) 
            ?? throw new InvalidOperationException("Trung tâm không tồn tại.");
        if (await _unitOfWork.Repository<User>().Query().AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã được sử dụng.");

        var user = new User
        {
            Id = 0,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Role = UserRole.Student,
            CenterId = centerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<User>().AddAsync(user);
        
        await _unitOfWork.Repository<Streak>().AddAsync(new Streak
        {
            Id = 0, UserId = user.Id,
            CurrentStreak = 0, LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow)
        });

        await _unitOfWork.SaveChangesAsync();
    }
}

using SignMate.Domain.Entities;

namespace SignMate.Application.Interfaces;

public interface IStreakService
{
    Task RecordActivityAsync(int userId);
    Task<Streak?> GetStreakAsync(int userId);
}

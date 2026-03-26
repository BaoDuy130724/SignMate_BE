using SignMate.Domain.Entities;

namespace SignMate.Application.Interfaces;

public interface IStreakService
{
    Task RecordActivityAsync(Guid userId);
    Task<Streak?> GetStreakAsync(Guid userId);
}

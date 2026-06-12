namespace SignMate.Application.Interfaces;

public interface ICurrentUser
{
    int UserId { get; }
    string Role { get; }
    int? CenterId { get; }
}

namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra khi thao tác vi phạm ràng buộc trạng thái hiện tại (ví dụ: email đã tồn tại,
/// đăng ký trùng). Ánh xạ sang HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message)
    {
    }
}

namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra khi người dùng đã xác thực nhưng không có quyền thực hiện thao tác.
/// Ánh xạ sang HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

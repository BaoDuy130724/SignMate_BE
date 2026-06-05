namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra khi một tính năng tạm thời không khả dụng (đang bảo trì / chưa cấu hình nhà cung cấp).
/// Ánh xạ sang HTTP 503 Service Unavailable.
/// </summary>
public sealed class ServiceUnavailableException : AppException
{
    public ServiceUnavailableException(string message) : base(message)
    {
    }
}

namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra cho lỗi nghiệp vụ phía client mang tính tổng quát (dữ liệu không hợp lệ về mặt
/// nghiệp vụ, ngoài phạm vi FluentValidation). Ánh xạ sang HTTP 400 Bad Request.
/// </summary>
public sealed class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message)
    {
    }
}

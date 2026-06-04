namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Lớp cơ sở cho các exception nghiệp vụ "có chủ đích" của ứng dụng.
/// Khác với exception hệ thống, các exception kế thừa lớp này được
/// <c>GlobalExceptionMiddleware</c> nhận diện và ánh xạ sang HTTP status code phù hợp,
/// trả về dưới dạng <c>ApiResponse</c> chuẩn thay vì lỗi 500.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}

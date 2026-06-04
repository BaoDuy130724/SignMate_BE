namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra khi không tìm thấy tài nguyên được yêu cầu. Ánh xạ sang HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Tạo thông điệp chuẩn hóa dạng "{name} (key) không tồn tại".
    /// </summary>
    /// <param name="name">Tên loại tài nguyên (ví dụ: "Course").</param>
    /// <param name="key">Khóa định danh đã tra cứu.</param>
    public NotFoundException(string name, object key)
        : base($"{name} ({key}) không tồn tại.")
    {
    }
}

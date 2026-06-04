namespace SignMate.Application.DTOs.Vocabulary;

/// <summary>
/// Kết quả trả về sau khi giáo viên tải lên video mẫu cho một từ vựng.
/// </summary>
public class UploadReferenceResultDto
{
    public int RequestId { get; set; }
}

/// <summary>
/// Một yêu cầu bơm video mẫu đang chờ quản trị viên duyệt (đã được AI tách keypoint xong).
/// </summary>
public class PendingReferenceRequestDto
{
    public int Id { get; set; }
    public string SignName { get; set; } = null!;
    public string UploaderName { get; set; } = null!;
    public string VideoUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

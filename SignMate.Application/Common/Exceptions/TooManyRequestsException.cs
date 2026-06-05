namespace SignMate.Application.Common.Exceptions;

/// <summary>
/// Ném ra khi client gửi yêu cầu quá nhanh/quá nhiều lần trong thời gian ngắn
/// (ví dụ: bấm "Gửi lại OTP" liên tục). Ánh xạ sang HTTP 429 Too Many Requests.
/// </summary>
public sealed class TooManyRequestsException : AppException
{
    public TooManyRequestsException(string message) : base(message)
    {
    }
}

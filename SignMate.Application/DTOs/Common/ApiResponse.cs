namespace SignMate.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null) 
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> FailureResult(string message, List<string>? errors = null) 
        => new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse SuccessResult(string? message = null) 
        => new() { Success = true, Message = message };

    public static ApiResponse FailureResult(string message, List<string>? errors = null) 
        => new() { Success = false, Message = message, Errors = errors };
}

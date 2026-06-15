using System.Net;
using System.Text.Json;
using FluentValidation;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Common;

namespace SignMate.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message, errors) = ex switch
        {
            // ── FluentValidation: gom toàn bộ lỗi field ────────────────────
            ValidationException valEx => (
                HttpStatusCode.BadRequest,
                "Dữ liệu không hợp lệ.",
                valEx.Errors.Select(e => e.ErrorMessage).Distinct().ToList()
            ),

            // ── Exception nghiệp vụ có chủ đích (ưu tiên khớp trước) ────────
            BadRequestException => (HttpStatusCode.BadRequest, ex.Message, (List<string>?)null),
            NotFoundException => (HttpStatusCode.NotFound, ex.Message, (List<string>?)null),
            ConflictException => (HttpStatusCode.Conflict, ex.Message, (List<string>?)null),
            ForbiddenException => (HttpStatusCode.Forbidden, ex.Message, (List<string>?)null),
            TooManyRequestsException => (HttpStatusCode.TooManyRequests, ex.Message, (List<string>?)null),
            ServiceUnavailableException => (HttpStatusCode.ServiceUnavailable, ex.Message, (List<string>?)null),

            // ── Fallback cho exception khung/hệ thống (giữ tương thích) ─────
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message, (List<string>?)null),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message, (List<string>?)null),
            InvalidOperationException => (HttpStatusCode.Conflict, ex.Message, (List<string>?)null),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message, (List<string>?)null),

            // ── Phân loại chi tiết cho các exception hệ thống ───────────────
            _ => ClassifySystemException(ex)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}", (int)statusCode, ex.Message);

        // Trong môi trường Development, bổ sung chi tiết exception gốc vào errors
        if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
        {
            errors ??= new List<string>();
            errors.Insert(0, $"[{ex.GetType().Name}] {ex.Message}");
            if (ex.InnerException != null)
                errors.Add($"[Inner: {ex.InnerException.GetType().Name}] {ex.InnerException.Message}");
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var apiResponse = ApiResponse.FailureResult(message, errors);
        var response = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(response);
    }

    /// <summary>
    /// Phân loại exception hệ thống thành các nhóm có thông báo rõ ràng,
    /// thay vì trả về chung chung "Đã xảy ra lỗi không mong muốn."
    /// </summary>
    private static (HttpStatusCode, string, List<string>?) ClassifySystemException(Exception ex)
    {
        // Lấy exception gốc sâu nhất (unwrap AggregateException nếu có)
        var root = ex is AggregateException agg ? agg.GetBaseException() : ex;
        var typeName = root.GetType().FullName ?? root.GetType().Name;

        // ── Database / EntityFramework ───────────────────────────────────
        if (typeName.Contains("DbUpdateConcurrencyException"))
            return (HttpStatusCode.Conflict,
                "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại và thử lại.",
                null);

        if (typeName.Contains("DbUpdateException") || typeName.Contains("SqlException"))
            return (HttpStatusCode.InternalServerError,
                "Lỗi khi lưu dữ liệu vào cơ sở dữ liệu. Vui lòng thử lại sau.",
                null);

        // ── Timeout ─────────────────────────────────────────────────────
        if (root is TimeoutException || root is TaskCanceledException tce && !tce.CancellationToken.IsCancellationRequested)
            return (HttpStatusCode.GatewayTimeout,
                "Yêu cầu mất quá nhiều thời gian để xử lý. Vui lòng thử lại sau.",
                null);

        // ── Client hủy request ──────────────────────────────────────────
        if (root is OperationCanceledException)
            return (HttpStatusCode.BadRequest,
                "Yêu cầu đã bị hủy bởi người dùng.",
                null);

        // ── PayOS / Third-party API exception (giữ message gốc) ────────
        if (typeName.Contains("PayOS") || typeName.Contains("ApiException"))
            return (HttpStatusCode.BadGateway,
                !string.IsNullOrWhiteSpace(root.Message) ? root.Message : "Lỗi từ cổng thanh toán. Vui lòng thử lại sau.",
                null);

        // ── Gọi service bên ngoài thất bại ──────────────────────────────
        if (root is HttpRequestException)
            return (HttpStatusCode.BadGateway,
                "Không thể kết nối đến dịch vụ bên ngoài. Vui lòng thử lại sau.",
                null);

        // ── File I/O ────────────────────────────────────────────────────
        if (root is IOException || root is FileNotFoundException)
            return (HttpStatusCode.InternalServerError,
                "Lỗi khi đọc/ghi tệp tin. Vui lòng thử lại sau.",
                null);

        // ── JSON parsing ────────────────────────────────────────────────
        if (root is JsonException)
            return (HttpStatusCode.BadRequest,
                "Dữ liệu gửi lên không đúng định dạng JSON.",
                null);

        // ── Null reference (lỗi lập trình) ──────────────────────────────
        if (root is NullReferenceException)
            return (HttpStatusCode.InternalServerError,
                "Lỗi hệ thống: tham chiếu đến dữ liệu không tồn tại. Vui lòng liên hệ hỗ trợ.",
                null);

        // ── Format / Overflow (dữ liệu đầu vào sai kiểu) ──────────────
        if (root is FormatException || root is OverflowException)
            return (HttpStatusCode.BadRequest,
                "Dữ liệu đầu vào không đúng định dạng hoặc vượt quá giới hạn cho phép.",
                null);

        // ── NotImplemented / NotSupported ────────────────────────────────
        if (root is NotImplementedException || root is NotSupportedException)
            return (HttpStatusCode.NotImplemented,
                "Chức năng này chưa được hỗ trợ.",
                null);

        // ── Fallback cuối cùng ──────────────────────────────────────────
        return (HttpStatusCode.InternalServerError,
            "Lỗi hệ thống nội bộ. Vui lòng thử lại sau hoặc liên hệ hỗ trợ nếu lỗi vẫn tiếp tục.",
            null);
    }
}


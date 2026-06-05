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

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
            _ => (HttpStatusCode.InternalServerError, "Đã xảy ra lỗi không mong muốn.", (List<string>?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            _logger.LogWarning("Handled exception ({StatusCode}): {Message}", (int)statusCode, ex.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var apiResponse = ApiResponse.FailureResult(message, errors);
        var response = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(response);
    }
}

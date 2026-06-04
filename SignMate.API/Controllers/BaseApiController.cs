using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Common;

namespace SignMate.API.Controllers;

/// <summary>
/// Lớp cơ sở cho mọi API controller trong hệ thống.
/// Mục tiêu: giữ controller "siêu mỏng" — chỉ điều phối request tới MediatR và bọc
/// kết quả vào <see cref="ApiResponse"/> chuẩn. Toàn bộ logic nghiệp vụ nằm ở
/// Command/Query Handler thuộc tầng Application.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Lazy-resolve <see cref="ISender"/> từ DI container của request hiện tại,
    /// giúp các controller dẫn xuất không phải khai báo constructor lặp lại.
    /// </summary>
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Trả về 200 OK với payload dữ liệu được bọc trong <see cref="ApiResponse{T}"/>.
    /// </summary>
    protected IActionResult Success<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.SuccessResult(data, message));

    /// <summary>
    /// Trả về 200 OK cho thao tác thành công không kèm dữ liệu (chỉ thông điệp).
    /// </summary>
    protected IActionResult Success(string? message = null) =>
        Ok(ApiResponse.SuccessResult(message));

    /// <summary>
    /// Trả về 201 Created với payload dữ liệu được bọc trong <see cref="ApiResponse{T}"/>.
    /// </summary>
    protected IActionResult Created<T>(T data, string? message = null) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<T>.SuccessResult(data, message));
}

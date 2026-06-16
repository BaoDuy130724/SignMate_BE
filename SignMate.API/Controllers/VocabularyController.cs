using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.Common.Exceptions;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Features.Vocabulary.Commands.ApproveReferenceRequest;
using SignMate.Application.Features.Vocabulary.Commands.RejectReferenceRequest;
using SignMate.Application.Features.Vocabulary.Commands.UploadReferenceVideo;
using SignMate.Application.Features.Vocabulary.Queries.GetPendingReferenceRequests;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý dữ liệu mẫu AI cho từ vựng: bơm video, tách keypoint ngầm và duyệt nội dung.
/// </summary>
[Route("api/vocabulary")]
[Authorize]
public class VocabularyController : BaseApiController
{
    /// <summary>
    /// Tải lên video mẫu cho từ vựng và khởi động xử lý ngầm tách keypoint.
    /// <c>POST /api/vocabulary/{signId}/upload-reference</c>.
    /// </summary>
    [HttpPost("{signId:int}/upload-reference")]
    [Authorize(Roles = "Teacher,CenterAdmin")]
    public async Task<IActionResult> UploadReferenceVideo(int signId, IFormFile video)
    {
        if (video is null || video.Length == 0)
            throw new BadRequestException("File video rỗng.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var stream = video.OpenReadStream();

        var result = await Mediator.Send(
            new UploadReferenceVideoCommand(signId, userId, stream, video.ContentType));

        return Success(result, "Tải video lên thành công. Hệ thống AI đang quét keypoint ngầm...");
    }

    /// <summary>Danh sách yêu cầu chờ duyệt. <c>GET /api/vocabulary/pending-references</c>.</summary>
    [HttpGet("pending-references")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var result = await Mediator.Send(new GetPendingReferenceRequestsQuery());
        return Success(result);
    }

    /// <summary>Duyệt yêu cầu bơm video. <c>POST /api/vocabulary/requests/{requestId}/approve</c>.</summary>
    [HttpPost("requests/{requestId:int}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveRequest(int requestId)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new ApproveReferenceRequestCommand(requestId, adminId));
        return Success("Đã duyệt! Dữ liệu mẫu AI của từ vựng đã được cập nhật.");
    }

    /// <summary>Từ chối yêu cầu bơm video. <c>POST /api/vocabulary/requests/{requestId}/reject</c>.</summary>
    [HttpPost("requests/{requestId:int}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectRequest(int requestId, [FromQuery] string reason = "Video không đạt chuẩn")
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await Mediator.Send(new RejectReferenceRequestCommand(requestId, adminId, reason));
        return Success("Đã từ chối yêu cầu.");
    }
}

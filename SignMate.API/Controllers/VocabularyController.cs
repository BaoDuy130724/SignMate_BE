using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignMate.Application.DTOs.Course;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/vocabulary")]
[Authorize]
public class VocabularyController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ISignMateDbContext _dbContext;
    private readonly IBlobService _blobService;
    private readonly IVideoProcessingQueue _taskQueue;

    public VocabularyController(
        ICourseService courseService, 
        ISignMateDbContext dbContext,
        IBlobService blobService,
        IVideoProcessingQueue taskQueue)
    {
        _courseService = courseService;
        _dbContext = dbContext;
        _blobService = blobService;
        _taskQueue = taskQueue;
    }

    [HttpPost("set-reference")]
    [Authorize(Roles = "SuperAdmin,CenterAdmin,Teacher")]
    public async Task<IActionResult> SetReference([FromBody] SetReferenceRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ReferenceKeypointData))
            return BadRequest(new { message = "Invalid ReferenceKeypointData payload." });

        var success = await _courseService.SetSignReferenceAsync(request);
        if (!success) return NotFound(new { message = "Sign not found to attach reference." });

        return Ok(new { message = "Reference keypoints set successfully." });
    }

    // ==============================================================
    // NEW ARCHITECTURE: ASYNC DATA PIPING WITH APPROVAL WORKFLOW
    // ==============================================================

    [HttpPost("{signId}/upload-reference")]
    [Authorize(Roles = "Teacher,CenterAdmin")]
    public async Task<IActionResult> UploadReferenceVideo(Guid signId, IFormFile video)
    {
        if (video == null || video.Length == 0) return BadRequest(new { message = "Empty video file." });
        
        var sign = await _dbContext.Signs.FindAsync(signId);
        if (sign == null) return NotFound(new { message = "Sign not found." });

        try
        {
            // 1. Upload video to temporary local storage / blob
            var fileName = $"reference_{signId}_{Guid.NewGuid()}.mp4";
            using var stream = video.OpenReadStream();
            var videoUrl = await _blobService.UploadAsync(stream, fileName, video.ContentType ?? "video/mp4");

            // 2. Create DB Request (Pending)
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var req = new SignReferenceRequest
            {
                Id = Guid.NewGuid(),
                SignId = signId,
                UploaderId = userId,
                VideoUrl = videoUrl,
                Status = ReferenceRequestStatus.Pending
            };

            _dbContext.SignReferenceRequests.Add(req);
            await _dbContext.SaveChangesAsync();

            // 3. Dispatch to Background Service
            await _taskQueue.QueueBackgroundWorkItemAsync(req.Id);

            return Ok(new { 
                message = "Tải Video lên thành công. Hệ thống AI đang quét keypoints ngầm...", 
                requestId = req.Id 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Upload failed: " + ex.Message });
        }
    }

    [HttpGet("pending-references")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var requests = await _dbContext.SignReferenceRequests
            .Include(x => x.Sign)
            .Include(x => x.Uploader)
            .Where(x => x.Status == ReferenceRequestStatus.ReadyForReview)
            .Select(x => new {
                x.Id,
                SignName = x.Sign.Word,
                UploaderName = x.Uploader.FullName,
                x.VideoUrl,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    [HttpPost("requests/{requestId}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveRequest(Guid requestId)
    {
        var request = await _dbContext.SignReferenceRequests
            .Include(x => x.Sign)
            .FirstOrDefaultAsync(x => x.Id == requestId);

        if (request == null) return NotFound(new { message = "Request not found." });
        if (request.Status != ReferenceRequestStatus.ReadyForReview) 
            return BadRequest(new { message = "Request is not ready for review." });

        if (string.IsNullOrEmpty(request.ExtractedKeypoints))
            return BadRequest(new { message = "Python AI hasn't returned keypoints yet or failed." });

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Update exact Sign entity
        request.Sign.ReferenceKeypointData = request.ExtractedKeypoints;
        request.Sign.VideoUrl = request.VideoUrl; // Keep video reference

        // Update request status
        request.Status = ReferenceRequestStatus.Approved;
        request.ReviewedById = adminId;
        request.ReviewedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Data Pump Approved! AI Learning Reference Updated successfully." });
    }

    [HttpPost("requests/{requestId}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectRequest(Guid requestId, [FromQuery] string reason = "Video không đạt chuẩn")
    {
        var request = await _dbContext.SignReferenceRequests.FindAsync(requestId);
        if (request == null) return NotFound();

        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        request.Status = ReferenceRequestStatus.Rejected;
        request.ReviewedById = adminId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewComment = reason;

        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Request rejected." });
    }
}

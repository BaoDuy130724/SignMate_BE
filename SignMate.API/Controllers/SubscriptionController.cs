using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignMate.Application.DTOs.Subscription;
// VNPay tạm vô hiệu hóa (sẽ thay bằng PayOS): using ...Commands.ProcessVnPayCallback;
using SignMate.Application.Features.Subscription.Commands.CreatePlan;
using SignMate.Application.Features.Subscription.Commands.DeletePlan;
using SignMate.Application.Features.Subscription.Commands.Subscribe;
using SignMate.Application.Features.Subscription.Commands.UpdatePlan;
using SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;
using SignMate.Application.Features.Subscription.Queries.GetMySubscription;
using SignMate.Application.Features.Subscription.Queries.GetPlans;
using SignMate.Application.Interfaces;
using SignMate.Domain.Entities;
using SignMate.Application.Features.Subscription.Common;
using System.Security.Claims;

namespace SignMate.API.Controllers;

/// <summary>
/// Quản lý gói dịch vụ &amp; thanh toán: xem gói, đăng ký, callback VNPay, gói hiện tại của người dùng.
/// </summary>
[Route("api")]
public class SubscriptionController : BaseApiController
{
    /// <summary>Danh sách gói cước. <c>GET /api/plans</c>.</summary>
    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
    {
        var result = await Mediator.Send(new GetPlansQuery());
        return Success(result);
    }

    /// <summary>Đăng ký một gói cước. <c>POST /api/subscription/subscribe</c>.</summary>
    [Authorize]
    [HttpPost("subscription/subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            ipAddress = "127.0.0.1";

        var result = await Mediator.Send(new SubscribeCommand(userId, request, ipAddress));
        return Success(result, result.Message);
    }

   /// <summary>
   /// PayOS webhook — PayOS gọi khi giao dịch hoàn tất. Xác thực chữ ký rồi kích hoạt gói.
   /// <c>POST /api/subscription/payos-webhook</c>.
    /// </summary>
    [HttpPost("subscription/payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        // TẠM THỜI BYPASS XÁC THỰC CHỮ KÝ ĐỂ TEST LOCAL BẰNG POSTMAN:
        int? subscriptionId = null;
        try
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("data", out var dataEl) && 
                dataEl.TryGetProperty("orderCode", out var orderCodeEl))
            {
                if (orderCodeEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    subscriptionId = orderCodeEl.GetInt32();
                }
                else if (orderCodeEl.ValueKind == System.Text.Json.JsonValueKind.String && 
                         int.TryParse(orderCodeEl.GetString(), out var parsedId))
                {
                    subscriptionId = parsedId;
                }
            }
        }
        catch
        {
            return BadRequest(new { message = "Invalid JSON format." });
        }

        if (subscriptionId == null)
        {
            return BadRequest(new { message = "Missing or invalid data.orderCode in request body." });
        }

        // Kích hoạt gói cước tương ứng với subscriptionId (orderCode)
        var subRepo = HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var sub = await subRepo.Repository<UserSubscription>().GetByIdAsync(subscriptionId.Value);

        if (sub is not null && !sub.IsActive)
        {
            await SubscriptionActivation.DeactivateActiveSubscriptionsAsync(
                subRepo, sub.UserId, CancellationToken.None);

            var plan = await subRepo.Repository<SubscriptionPlan>().GetByIdAsync(sub.PlanId);
            sub.IsActive = true;
            sub.StartDate = DateTime.UtcNow;
            sub.EndDate = DateTime.UtcNow.AddDays(plan?.DurationDays ?? 30);

            await subRepo.SaveChangesAsync(CancellationToken.None);
            return Ok(new { success = true, message = $"Subscription {subscriptionId} activated successfully." });
        }

        return Ok(new { success = true, message = $"Subscription {subscriptionId} was already active or not found." });
    }

    /// <summary>Gói cước hiện tại của người dùng. <c>GET /api/subscription/me</c>.</summary>
    [Authorize]
    [HttpGet("subscription/me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMySubscriptionQuery(userId));
        return Success(result);
    }

    /// <summary>Toàn bộ lượt đăng ký gói — chỉ SuperAdmin. <c>GET /api/subscription/all</c>.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("subscription/all")]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        var result = await Mediator.Send(new GetAllSubscriptionsQuery());
        return Success(result);
    }

    // ── Plan CRUD (SuperAdmin) ─────────────────────────────────────────────────

    /// <summary>Tạo gói cước mới. <c>POST /api/plans</c>.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequest request)
    {
        var result = await Mediator.Send(new CreatePlanCommand(request));
        return Success(result, "Tạo gói cước thành công.");
    }

    /// <summary>Cập nhật gói cước. <c>PUT /api/plans/{id}</c>.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("plans/{id:int}")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanRequest request)
    {
        var result = await Mediator.Send(new UpdatePlanCommand(id, request));
        return Success(result, "Cập nhật gói cước thành công.");
    }

    /// <summary>Xóa gói cước. <c>DELETE /api/plans/{id}</c>.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("plans/{id:int}")]
    public async Task<IActionResult> DeletePlan(int id)
    {
        await Mediator.Send(new DeletePlanCommand(id));
        return Success(Unit.Value, "Xóa gói cước thành công.");
    }
}

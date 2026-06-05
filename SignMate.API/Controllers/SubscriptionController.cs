using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Subscription;
// VNPay tạm vô hiệu hóa (sẽ thay bằng PayOS): using ...Commands.ProcessVnPayCallback;
using SignMate.Application.Features.Subscription.Commands.Subscribe;
using SignMate.Application.Features.Subscription.Queries.GetAllSubscriptions;
using SignMate.Application.Features.Subscription.Queries.GetMySubscription;
using SignMate.Application.Features.Subscription.Queries.GetPlans;

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

    // ── VNPay callback endpoints TẠM VÔ HIỆU HÓA (chờ tích hợp PayOS) ─────────────
    // Handler ProcessVnPayCallback + VnPayService vẫn còn trong code để tham chiếu.
    // Khi gắn PayOS, thay 2 endpoint dưới đây bằng return/webhook tương ứng của PayOS.
    /*
    /// <summary>
    /// VNPay redirect callback — người dùng quay lại sau khi thanh toán. Trả về HTML để Flutter
    /// WebView phát hiện kết quả (đây là contract bên ngoài nên không bọc ApiResponse).
    /// <c>GET /api/subscription/vnpay-return</c>.
    /// </summary>
    [HttpGet("subscription/vnpay-return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var parameters = HttpContext.Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        var outcome = await Mediator.Send(new ProcessVnPayCallbackCommand(parameters));

        if (!outcome.IsValid)
            return BadRequest(new { message = "Chữ ký VNPay không hợp lệ." });

        var html = BuildResultHtml(outcome.IsSuccess);
        return Content(html, "text/html");
    }

    /// <summary>
    /// VNPay IPN (server-to-server) — VNPay gọi để xác nhận thanh toán. Trả về RspCode theo chuẩn VNPay.
    /// <c>GET /api/subscription/vnpay-ipn</c>.
    /// </summary>
    [HttpGet("subscription/vnpay-ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var parameters = HttpContext.Request.Query.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        var outcome = await Mediator.Send(new ProcessVnPayCallbackCommand(parameters));

        return outcome.IsValid
            ? Ok(new { RspCode = "00", Message = "Confirm success" })
            : Ok(new { RspCode = "97", Message = "Invalid signature" });
    }
    */

    /// <summary>Gói cước hiện tại của người dùng. <c>GET /api/subscription/me</c>.</summary>
    [Authorize]
    [HttpGet("subscription/me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await Mediator.Send(new GetMySubscriptionQuery(userId));
        return Success(result);
    }

    /// <summary>Toàn bộ lượt đăng ký gói (quản trị). <c>GET /api/subscription/all</c>.</summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("subscription/all")]
    public async Task<IActionResult> GetAllSubscriptions()
    {
        var result = await Mediator.Send(new GetAllSubscriptionsQuery());
        return Success(result);
    }

    /* VNPay tạm vô hiệu hóa — giữ lại helper dựng HTML kết quả để tham chiếu khi làm PayOS.
    /// <summary>
    /// Dựng trang HTML kết quả thanh toán cho WebView của app phát hiện trạng thái.
    /// </summary>
    private static string BuildResultHtml(bool isSuccess)
    {
        var icon = isSuccess ? "✅" : "❌";
        var color = isSuccess ? "#4CAF50" : "#F44336";
        var title = isSuccess ? "Thanh toán thành công!" : "Thanh toán thất bại";
        var status = isSuccess ? "success" : "failed";

        return $@"
<!DOCTYPE html>
<html>
<head><meta name=""viewport"" content=""width=device-width, initial-scale=1""></head>
<body style=""display:flex;align-items:center;justify-content:center;min-height:100vh;font-family:sans-serif;background:#f5f5f5"">
  <div style=""text-align:center;padding:40px;background:white;border-radius:20px;box-shadow:0 4px 20px rgba(0,0,0,0.1)"">
    <div style=""font-size:64px"">{icon}</div>
    <h2 style=""color:{color}"">{title}</h2>
    <p style=""color:#666"">Bạn có thể đóng trang này và quay lại ứng dụng.</p>
    <p id=""status"" style=""display:none"">{status}</p>
  </div>
</body>
</html>";
    }
    */
}

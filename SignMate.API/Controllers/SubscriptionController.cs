using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.Subscription;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subService;
    private readonly IVnPayService _vnPayService;

    public SubscriptionController(ISubscriptionService subService, IVnPayService vnPayService)
    {
        _subService = subService;
        _vnPayService = vnPayService;
    }

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
        => Ok(await _subService.GetPlansAsync());

    [Authorize]
    [HttpPost("subscription/subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
        {
            ipAddress = "127.0.0.1";
        }

        var res = await _subService.SubscribeAsync(userId, request, ipAddress);
        return res.Success ? Ok(res) : BadRequest(new { message = res.Message });
    }
    /// <summary>
    /// VNPay redirect callback — user returns here after payment.
    /// </summary>
    [HttpGet("subscription/vnpay-return")]
    public async Task<IActionResult> VnPayReturn()
    {
        var vnpayParams = HttpContext.Request.Query
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        var result = _vnPayService.ValidateCallback(vnpayParams);

        if (!result.IsValid)
            return BadRequest(new { message = "Invalid VNPay signature" });

        if (result.IsSuccess)
        {
            // Find subscription by TxnRef pattern
            var txnRef = vnpayParams.GetValueOrDefault("vnp_TxnRef") ?? "";
            var sub = await FindSubscriptionByTxnRef(txnRef);

            if (sub.HasValue)
                await _subService.ConfirmPaymentAsync(sub.Value);
        }

        // Return a simple HTML page that the Flutter WebView can detect
        var status = result.IsSuccess ? "success" : "failed";
        var html = $@"
<!DOCTYPE html>
<html>
<head><meta name=""viewport"" content=""width=device-width, initial-scale=1""></head>
<body style=""display:flex;align-items:center;justify-content:center;min-height:100vh;font-family:sans-serif;background:#f5f5f5"">
  <div style=""text-align:center;padding:40px;background:white;border-radius:20px;box-shadow:0 4px 20px rgba(0,0,0,0.1)"">
    <div style=""font-size:64px"">{(result.IsSuccess ? "✅" : "❌")}</div>
    <h2 style=""color:{(result.IsSuccess ? "#4CAF50" : "#F44336")}"">{(result.IsSuccess ? "Thanh toán thành công!" : "Thanh toán thất bại")}</h2>
    <p style=""color:#666"">Bạn có thể đóng trang này và quay lại ứng dụng.</p>
    <p id=""status"" style=""display:none"">{status}</p>
  </div>
</body>
</html>";

        return Content(html, "text/html");
    }

    /// <summary>
    /// VNPay IPN (server-to-server) — VNPay calls this to confirm payment.
    /// </summary>
    [HttpGet("subscription/vnpay-ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var vnpayParams = HttpContext.Request.Query
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        var result = _vnPayService.ValidateCallback(vnpayParams);

        if (!result.IsValid)
            return Ok(new { RspCode = "97", Message = "Invalid signature" });

        if (result.IsSuccess)
        {
            var txnRef = vnpayParams.GetValueOrDefault("vnp_TxnRef") ?? "";
            var sub = await FindSubscriptionByTxnRef(txnRef);

            if (sub.HasValue)
                await _subService.ConfirmPaymentAsync(sub.Value);
        }

        return Ok(new { RspCode = "00", Message = "Confirm success" });
    }

    [Authorize]
    [HttpGet("subscription/me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sub = await _subService.GetMySubscriptionAsync(userId);
        return sub == null ? NotFound() : Ok(sub);
    }

    private async Task<Guid?> FindSubscriptionByTxnRef(string txnRef)
    {
        if (string.IsNullOrEmpty(txnRef) || txnRef.Length < 32) return null;

        if (Guid.TryParseExact(txnRef, "N", out var subId))
            return await Task.FromResult(subId);

        return null;
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("subscription/all")]
    public async Task<IActionResult> GetAllSubscriptions()
        => Ok(await _subService.GetAllSubscriptionsAsync());
}

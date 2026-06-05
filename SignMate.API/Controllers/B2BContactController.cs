using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SignMate.Application.DTOs.B2BContact;
using SignMate.Application.Features.B2BContact.Commands.SubmitContact;

namespace SignMate.API.Controllers;

/// <summary>
/// Tiếp nhận lead B2B từ biểu mẫu liên hệ doanh nghiệp/trường học của app mobile.
/// </summary>
[Route("api/b2b/contact")]
public class B2BContactController : BaseApiController
{
    /// <summary>
    /// Ghi nhận thông tin khách hàng tiềm năng B2B. <c>POST /api/b2b/contact</c>.
    /// </summary>
    [EnableRateLimiting("auth")]
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateB2BContactRequest request)
    {
        var lead = await Mediator.Send(new SubmitContactCommand(request));
        return Created(lead, "Đã gửi thông tin liên hệ thành công. Chúng tôi sẽ phản hồi trong 24 giờ.");
    }
}

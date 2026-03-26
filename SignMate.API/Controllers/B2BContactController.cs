using Microsoft.AspNetCore.Mvc;
using SignMate.Application.DTOs.B2BContact;
using SignMate.Application.Interfaces;

namespace SignMate.API.Controllers;

[ApiController]
[Route("api/b2b/contact")]
public class B2BContactController : ControllerBase
{
    private readonly IB2BContactService _contactService;

    public B2BContactController(IB2BContactService contactService)
        => _contactService = contactService;

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateB2BContactRequest request)
    {
        var lead = await _contactService.SubmitContactFormAsync(request);
        return Created("", lead);
    }
}

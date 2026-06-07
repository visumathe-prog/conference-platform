using Microsoft.AspNetCore.Mvc;
using Registration.Service.Models;
using Registration.Service.Services;

namespace Registration.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("{eventId}/register")]
    public async Task<IActionResult> Register(Guid eventId, [FromBody] RegistrationRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
        var result = await _registrationService.RegisterAsync(eventId, userId, request.TicketType);
        
        if (result)
            return Ok(new { message = "Registration successful" });
        
        return BadRequest(new { message = "No seats available" });
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = Guid.Parse(User.FindFirst("userId")?.Value ?? Guid.Empty.ToString());
        var tickets = await _registrationService.GetUserRegistrationsAsync(userId);
        return Ok(tickets);
    }
}

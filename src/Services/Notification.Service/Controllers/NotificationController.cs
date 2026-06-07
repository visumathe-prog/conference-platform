using Microsoft.AspNetCore.Mvc;
using Notification.Service.Models;
using Notification.Service.Services;

namespace Notification.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        await _notificationService.SendEmailAsync(request.To, request.Subject, request.Body);
        return Ok();
    }
}

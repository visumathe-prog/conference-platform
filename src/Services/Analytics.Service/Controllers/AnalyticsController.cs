using Microsoft.AspNetCore.Mvc;
using Analytics.Service.Services;
using Analytics.Service.Models;

namespace Analytics.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("events/{eventId}/stats")]
    public async Task<IActionResult> GetEventStats(Guid eventId)
    {
        var stats = await _analyticsService.GetEventStatisticsAsync(eventId);
        return Ok(stats);
    }

    [HttpGet("events/top")]
    public async Task<IActionResult> GetTopEvents([FromQuery] int limit = 10)
    {
        var topEvents = await _analyticsService.GetTopEventsAsync(limit);
        return Ok(topEvents);
    }

    [HttpGet("daily-registrations")]
    public async Task<IActionResult> GetDailyRegistrations([FromQuery] int days = 7)
    {
        var daily = await _analyticsService.GetDailyRegistrationsAsync(days);
        return Ok(daily);
    }
}

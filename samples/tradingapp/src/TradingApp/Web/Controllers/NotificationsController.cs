using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Application.DTOs;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationsListResponse>> Get([FromQuery] bool unreadOnly = false, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var items = await _notificationService.GetRecentAsync(Math.Clamp(limit, 1, 100), unreadOnly, ct);
        return Ok(new NotificationsListResponse(items.ToList()));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await _notificationService.MarkReadAsync(id, ct);
        return NoContent();
    }
}

public record NotificationsListResponse(List<NotificationDto> Items);

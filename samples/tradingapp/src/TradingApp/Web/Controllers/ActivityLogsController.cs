using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/activity-logs")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogRepository _logRepository;
    private readonly ILogger<ActivityLogsController> _logger;

    public ActivityLogsController(IActivityLogRepository logRepository, ILogger<ActivityLogsController> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ActivityLogsResponse>> GetLogs(
        [FromQuery] int limit = 100,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        try
        {
            var logs = string.IsNullOrWhiteSpace(category)
                ? await _logRepository.GetRecentAsync(limit, ct)
                : await _logRepository.GetByCategoryAsync(category, limit, ct);

            return Ok(new ActivityLogsResponse
            {
                Logs = logs.Select(l => new ActivityLogDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    Level = l.Level,
                    Category = l.Category,
                    Symbol = l.Symbol,
                    AlertId = l.AlertId,
                    Message = l.Message,
                    Details = l.Details
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get activity logs");
            return StatusCode(500, new ApiErrorResponse($"Failed to get activity logs: {ex.Message}", Array.Empty<string>()));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll(CancellationToken ct = default)
    {
        try
        {
            await _logRepository.DeleteAllAsync(ct);
            _logger.LogInformation("All activity logs deleted");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all activity logs");
            return StatusCode(500, new ApiErrorResponse($"Failed to delete all activity logs: {ex.Message}", Array.Empty<string>()));
        }
    }
}

public sealed class ActivityLogsResponse
{
    [JsonPropertyName("logs")]
    public List<ActivityLogDto> Logs { get; set; } = new();
}

public sealed class ActivityLogDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    [JsonPropertyName("alertId")]
    public string? AlertId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}

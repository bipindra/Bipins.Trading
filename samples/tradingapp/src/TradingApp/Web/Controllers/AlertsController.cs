using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AlertService _alertService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(AlertService alertService, ILogger<AlertsController> logger)
    {
        _alertService = alertService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AlertsListResponse>> Get([FromQuery] string? symbol, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new ApiErrorResponse("Symbol query parameter is required.", new[] { "symbol" }));
        var alerts = await _alertService.GetBySymbolAsync(symbol.Trim(), ct);
        return Ok(new AlertsListResponse(alerts.ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<AlertDto>> Post([FromBody] AlertRequest? body, CancellationToken ct)
    {
        _logger.LogInformation("POST /api/alerts called. Body: {Body}", body != null ? System.Text.Json.JsonSerializer.Serialize(body) : "null");
        
        if (body == null)
        {
            _logger.LogWarning("POST /api/alerts: Request body is null");
            return BadRequest(new ApiErrorResponse("Request body is required.", new[] { "body" }));
        }
        
        if (string.IsNullOrWhiteSpace(body.Symbol))
        {
            _logger.LogWarning("POST /api/alerts: Symbol is null or empty");
            return BadRequest(new ApiErrorResponse("Symbol is required.", new[] { "Symbol" }));
        }
        
        _logger.LogInformation("POST /api/alerts: Creating alert for symbol {Symbol}, type {AlertType}", body.Symbol, body.AlertType);
        
        try
        {
            var alert = await _alertService.CreateAsync(body, ct);
            _logger.LogInformation("POST /api/alerts: Alert created successfully with ID {Id} for symbol {Symbol}", alert.Id, alert.Symbol);
            return StatusCode(201, alert);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "POST /api/alerts: ArgumentException - {Message}", ex.Message);
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST /api/alerts: Exception occurred - {Message}. StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
            return StatusCode(500, new ApiErrorResponse($"Failed to create alert: {ex.Message}", Array.Empty<string>()));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        _logger.LogInformation("Delete alert endpoint called with id: {Id}", id);
        try
        {
            var deleted = await _alertService.DeleteAsync(id, ct);
            if (!deleted)
            {
                _logger.LogWarning("Alert {Id} not found for deletion", id);
                return NotFound(new ApiErrorResponse("Alert not found.", Array.Empty<string>()));
            }
            _logger.LogInformation("Alert {Id} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert {Id}", id);
            return StatusCode(500, new ApiErrorResponse($"Failed to delete alert: {ex.Message}", Array.Empty<string>()));
        }
    }
}

public record AlertsListResponse(List<AlertDto> Items);

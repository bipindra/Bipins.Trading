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

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
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
        if (body == null || string.IsNullOrWhiteSpace(body.Symbol))
            return BadRequest(new ApiErrorResponse("Symbol and AlertType are required.", new[] { "Symbol" }));
        try
        {
            var alert = await _alertService.CreateAsync(body, ct);
            return StatusCode(201, alert);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }
}

public record AlertsListResponse(List<AlertDto> Items);

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/signals")]
public class SignalsController : ControllerBase
{
    private readonly ISignalService _signalService;
    private readonly ILogger<SignalsController> _logger;

    public SignalsController(ISignalService signalService, ILogger<SignalsController> logger)
    {
        _signalService = signalService;
        _logger = logger;
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<SignalsResponse>> GetSignals(
        string symbol,
        [FromQuery] string? strategy = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("GetSignals endpoint called for symbol: {Symbol}, strategy: {Strategy}", symbol, strategy ?? "all");
        
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new ApiErrorResponse("Symbol is required.", new[] { "symbol" }));

        try
        {
            var signals = await _signalService.GetSignalsAsync(symbol.Trim(), strategy, ct);
            _logger.LogInformation("Found {Count} signals for {Symbol}", signals.Count, symbol);
            return Ok(new SignalsResponse
            {
                Symbol = symbol.Trim().ToUpperInvariant(),
                Signals = signals.Select(s => new SignalResponseDto
                {
                    Strategy = s.Strategy,
                    Symbol = s.Symbol,
                    Time = s.Time,
                    SignalType = s.SignalType,
                    Price = s.Price,
                    Reason = s.Reason
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signals for {Symbol}", symbol);
            return StatusCode(500, new ApiErrorResponse($"Failed to get signals: {ex.Message}", Array.Empty<string>()));
        }
    }
}

public sealed class SignalsResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("signals")]
    public List<SignalResponseDto> Signals { get; set; } = new();
}

public sealed class SignalResponseDto
{
    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("signalType")]
    public string SignalType { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

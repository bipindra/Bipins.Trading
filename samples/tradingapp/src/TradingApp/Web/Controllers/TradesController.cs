using System.Text.Json.Serialization;
using Bipins.Trading.Domain;
using Bipins.Trading.Execution;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly IExecutionAdapter _executionAdapter;
    private readonly IAlpacaService _alpacaService;
    private readonly ILogger<TradesController> _logger;

    public TradesController(
        IExecutionAdapter executionAdapter,
        IAlpacaService alpacaService,
        ILogger<TradesController> logger)
    {
        _executionAdapter = executionAdapter;
        _alpacaService = alpacaService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TradeResponse>> ExecuteTrade([FromBody] TradeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return BadRequest(new ApiErrorResponse("Symbol is required.", new[] { "symbol" }));

        if (!request.Quantity.HasValue || request.Quantity.Value <= 0)
            return BadRequest(new ApiErrorResponse("Quantity must be greater than 0.", new[] { "quantity" }));

        if (request.OrderType == OrderType.Limit && !request.LimitPrice.HasValue)
            return BadRequest(new ApiErrorResponse("Limit price is required for limit orders.", new[] { "limitPrice" }));

        if (request.OrderType == OrderType.Stop && !request.StopPrice.HasValue)
            return BadRequest(new ApiErrorResponse("Stop price is required for stop orders.", new[] { "stopPrice" }));

        if (request.OrderType == OrderType.StopLimit && (!request.LimitPrice.HasValue || !request.StopPrice.HasValue))
            return BadRequest(new ApiErrorResponse("Both limit price and stop price are required for stop-limit orders.", new[] { "limitPrice", "stopPrice" }));

        try
        {
            // Store expiration date in Metrics dictionary for GTD orders
            var metrics = request.ExpirationDate.HasValue && request.TimeInForce == TimeInForce.GTC
                ? new Dictionary<string, decimal> { { "expiration_date_ticks", request.ExpirationDate.Value.Ticks } }
                : null;

            var orderIntent = new OrderIntent(
                Strategy: "TradingApp.Manual",
                Symbol: request.Symbol.Trim().ToUpperInvariant(),
                Time: DateTime.UtcNow,
                Side: request.Side,
                OrderType: request.OrderType,
                TimeInForce: request.TimeInForce,
                Quantity: request.Quantity,
                LimitPrice: request.LimitPrice,
                StopPrice: request.StopPrice,
                RiskEnvelope: null,
                Reason: $"Manual {request.Side} order",
                Metrics: metrics,
                ClientOrderId: $"manual-{Guid.NewGuid():N}");

            await _executionAdapter.SubmitAsync(orderIntent, ct);

            _logger.LogInformation("Trade executed: {Side} {Quantity} {Symbol} {OrderType}", 
                request.Side, request.Quantity, request.Symbol, request.OrderType);

            return Ok(new TradeResponse
            {
                Success = true,
                Message = $"Order submitted successfully",
                ClientOrderId = orderIntent.ClientOrderId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute trade for {Symbol}", request.Symbol);
            return StatusCode(500, new ApiErrorResponse($"Failed to execute trade: {ex.Message}", Array.Empty<string>()));
        }
    }

    [HttpGet("{symbol}/intraday")]
    public async Task<ActionResult<IntradayBarsResponse>> GetIntradayBars(
        string symbol,
        [FromQuery] string? timeframe = "1Min",
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new ApiErrorResponse("Symbol is required.", new[] { "symbol" }));

        try
        {
            var today = DateTime.UtcNow.Date;
            var start = today.AddHours(-9); // Market open (9:30 AM ET = 13:30 UTC, but we'll get more data)
            var end = DateTime.UtcNow;

            var bars = await _alpacaService.GetBarsAsync(symbol.Trim(), timeframe ?? "1Min", start, end, ct);
            
            return Ok(new IntradayBarsResponse
            {
                Symbol = symbol.Trim().ToUpperInvariant(),
                Bars = bars.Select(b => new BarDto
                {
                    Time = b.Time,
                    Open = b.Open,
                    High = b.High,
                    Low = b.Low,
                    Close = b.Close,
                    Volume = b.Volume
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get intraday bars for {Symbol}", symbol);
            return StatusCode(500, new ApiErrorResponse($"Failed to get intraday bars: {ex.Message}", Array.Empty<string>()));
        }
    }
}

public sealed class TradeRequest
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public OrderSide Side { get; set; }

    [JsonPropertyName("orderType")]
    public OrderType OrderType { get; set; }

    [JsonPropertyName("timeInForce")]
    public TimeInForce TimeInForce { get; set; } = TimeInForce.Day;

    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }

    [JsonPropertyName("limitPrice")]
    public decimal? LimitPrice { get; set; }

    [JsonPropertyName("stopPrice")]
    public decimal? StopPrice { get; set; }

    [JsonPropertyName("expirationDate")]
    public DateTime? ExpirationDate { get; set; } // For GTD orders
}

public sealed class TradeResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("clientOrderId")]
    public string? ClientOrderId { get; set; }
}

public sealed class IntradayBarsResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("bars")]
    public List<BarDto> Bars { get; set; } = new();
}

public sealed class BarDto
{
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("open")]
    public decimal Open { get; set; }

    [JsonPropertyName("high")]
    public decimal High { get; set; }

    [JsonPropertyName("low")]
    public decimal Low { get; set; }

    [JsonPropertyName("close")]
    public decimal Close { get; set; }

    [JsonPropertyName("volume")]
    public decimal Volume { get; set; }
}

using Bipins.Trading.Domain;
using Bipins.Trading.Execution;
using Microsoft.Extensions.Logging;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Web.Services;

/// <summary>
/// When an alert triggers, submits a live order via the Alpaca Trading API (Market, 1 share at trigger price).
/// Direction: PriceAbove -> Buy, PriceBelow -> Sell.
/// </summary>
public sealed class BipinsExecutionEngine : IExecutionEngine
{
    private readonly IExecutionAdapter _adapter;
    private readonly ILogger<BipinsExecutionEngine> _logger;

    public BipinsExecutionEngine(IExecutionAdapter adapter, ILogger<BipinsExecutionEngine> logger)
    {
        _adapter = adapter;
        _logger = logger;
    }

    public Task ExecuteOnTriggerAsync(Alert alert, decimal triggerPrice, CancellationToken ct = default)
    {
        var side = (alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.RsiOversold)
            ? OrderSide.Buy
            : OrderSide.Sell;
        var intent = new OrderIntent(
            Strategy: "TradingApp.Alert",
            Symbol: alert.Symbol,
            Time: DateTime.UtcNow,
            Side: side,
            OrderType: OrderType.Market,
            TimeInForce: TimeInForce.Day,
            Quantity: 1m,
            LimitPrice: triggerPrice,
            StopPrice: null,
            RiskEnvelope: null,
            Reason: $"Alert {alert.Id} triggered at {triggerPrice:F2}",
            Metrics: null,
            ClientOrderId: $"alert-{alert.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}");

        _logger.LogInformation("Executing live order: {Side} {Quantity} {Symbol} @ {Price}", side, 1m, alert.Symbol, triggerPrice);
        return _adapter.SubmitAsync(intent, ct);
    }
}

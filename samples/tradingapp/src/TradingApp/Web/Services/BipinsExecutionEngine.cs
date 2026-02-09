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
        // Use configured order side override, or default based on alert type
        var side = alert.OrderSideOverride ?? 
            ((alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.RsiOversold)
                ? OrderSide.Buy
                : OrderSide.Sell);
        
        // Use configured order type, or default to Market
        var orderType = alert.OrderType ?? OrderType.Market;
        
        // Use configured quantity, or default to 1
        var quantity = alert.OrderQuantity ?? 1m;
        
        // Use configured time in force, or default to Day
        var timeInForce = alert.OrderTimeInForce ?? TimeInForce.Day;
        
        // For limit/stop orders, use configured prices; for market, use trigger price as reference
        var limitPrice = orderType == OrderType.Market 
            ? triggerPrice 
            : (alert.OrderLimitPrice ?? (orderType == OrderType.Limit ? triggerPrice : null));
        var stopPrice = alert.OrderStopPrice;
        
        var intent = new OrderIntent(
            Strategy: "TradingApp.Alert",
            Symbol: alert.Symbol,
            Time: DateTime.UtcNow,
            Side: side,
            OrderType: orderType,
            TimeInForce: timeInForce,
            Quantity: quantity,
            LimitPrice: limitPrice,
            StopPrice: stopPrice,
            RiskEnvelope: null,
            Reason: $"Alert {alert.Id} triggered at {triggerPrice:F2}",
            Metrics: null,
            ClientOrderId: $"alert-{alert.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}");

        _logger.LogInformation("Executing live order: {Side} {Quantity} {Symbol} @ {Price} (Type: {OrderType})", 
            side, quantity, alert.Symbol, triggerPrice, orderType);
        return _adapter.SubmitAsync(intent, ct);
    }
}

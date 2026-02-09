using Bipins.Trading.Domain;
using TradingApp.Domain;

namespace TradingApp.Application.DTOs;

public record AlertRequest(
    string Symbol, 
    AlertType AlertType, 
    string? Payload = null,
    decimal? Threshold = null,
    ComparisonType? ComparisonType = null,
    string? Timeframe = null,
    bool EnableAutoExecute = false,
    decimal? OrderQuantity = null,
    OrderType? OrderType = null,
    OrderSide? OrderSideOverride = null,
    decimal? OrderLimitPrice = null,
    decimal? OrderStopPrice = null,
    TimeInForce? OrderTimeInForce = null);

namespace Bipins.Trading.Domain;

public readonly record struct OrderIntent(
    string Strategy,
    string Symbol,
    DateTime Time,
    OrderSide Side,
    OrderType OrderType,
    TimeInForce TimeInForce,
    decimal? Quantity,
    decimal? LimitPrice,
    decimal? StopPrice,
    RiskEnvelope? RiskEnvelope,
    string? Reason,
    IReadOnlyDictionary<string, decimal>? Metrics,
    string? ClientOrderId);

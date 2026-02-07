namespace Bipins.AI.Trading.Domain;

public readonly record struct RiskEnvelope(
    decimal? StopLoss = null,
    decimal? TakeProfit = null,
    decimal? TrailingStopDistance = null);

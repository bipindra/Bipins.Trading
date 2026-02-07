namespace Bipins.AI.Trading.Domain;

public sealed class StrategyContext
{
    public required string Symbol { get; init; }
    public required string Timeframe { get; init; }
    public required IReadOnlyList<Candle> Candles { get; init; }
    public required PortfolioState Portfolio { get; init; }
    public required Position CurrentPosition { get; init; }
    public required IIndicatorProvider Indicators { get; init; }
    public required IReadOnlyDictionary<string, object> Parameters { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<Candle>>? CandlesByTimeframe { get; init; }
}

namespace Bipins.Trading.Domain;

public sealed class StrategyContext
{
#if NET7_0_OR_GREATER
    public required string Symbol { get; init; }
    public required string Timeframe { get; init; }
    public required IReadOnlyList<Candle> Candles { get; init; }
    public required PortfolioState Portfolio { get; init; }
    public required Position CurrentPosition { get; init; }
    public required IIndicatorProvider Indicators { get; init; }
    public required IReadOnlyDictionary<string, object> Parameters { get; init; }
#else
    public string Symbol { get; init; } = string.Empty;
    public string Timeframe { get; init; } = string.Empty;
    public IReadOnlyList<Candle> Candles { get; init; } = Array.Empty<Candle>();
    public PortfolioState Portfolio { get; init; }
    public Position CurrentPosition { get; init; }
    public IIndicatorProvider Indicators { get; init; } = null!;
    public IReadOnlyDictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();
#endif
    public CancellationToken CancellationToken { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<Candle>>? CandlesByTimeframe { get; init; }
}

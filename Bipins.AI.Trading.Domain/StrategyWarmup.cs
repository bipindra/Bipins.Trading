namespace Bipins.AI.Trading.Domain;

public readonly record struct StrategyWarmup(
    int RequiredCandleCount = 0,
    IReadOnlyList<string>? Timeframes = null)
{
    public static StrategyWarmup None => new(0, null);
    public static StrategyWarmup Bars(int count) => new(count, null);
}

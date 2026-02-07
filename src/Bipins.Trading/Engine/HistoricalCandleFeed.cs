using Bipins.Trading.Domain;

namespace Bipins.Trading.Engine;

public sealed class HistoricalCandleFeed : IMarketDataFeed
{
    private readonly Func<string, string, DateTime, DateTime, IEnumerable<Candle>> _source;

    public HistoricalCandleFeed(Func<string, string, DateTime, DateTime, IEnumerable<Candle>> source)
    {
        _source = source;
    }

    public static HistoricalCandleFeed FromCandles(IReadOnlyList<Candle> candles, string symbol, string timeframe)
    {
        return new HistoricalCandleFeed((s, tf, start, end) =>
        {
            if (s != symbol || tf != timeframe) return Array.Empty<Candle>();
            return candles.Where(c => c.Time >= start && c.Time <= end).OrderBy(c => c.Time);
        });
    }

    public IEnumerable<Candle> GetCandles(string symbol, string timeframe, DateTime start, DateTime end) =>
        _source(symbol, timeframe, start, end);
}

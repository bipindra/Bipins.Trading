using Bipins.Trading.Domain;

namespace Bipins.Trading.Engine;

public interface IMarketDataFeed
{
    IEnumerable<Candle> GetCandles(string symbol, string timeframe, DateTime start, DateTime end);
}

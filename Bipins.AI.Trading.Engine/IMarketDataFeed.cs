using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Engine;

public interface IMarketDataFeed
{
    IEnumerable<Candle> GetCandles(string symbol, string timeframe, DateTime start, DateTime end);
}

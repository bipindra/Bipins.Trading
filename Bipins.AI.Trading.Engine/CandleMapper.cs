using Bipins.AI.Trading.Domain;
using IndCandle = Bipins.AI.Trading.Indicators.Models.Candle;

namespace Bipins.AI.Trading.Engine;

public static class CandleMapper
{
    public static IndCandle ToIndicatorCandle(Candle d) =>
        new(d.Time, d.Open, d.High, d.Low, d.Close, d.Volume);

    public static IEnumerable<IndCandle> ToIndicatorCandles(IReadOnlyList<Candle> domainCandles)
    {
        foreach (var d in domainCandles)
            yield return ToIndicatorCandle(d);
    }
}

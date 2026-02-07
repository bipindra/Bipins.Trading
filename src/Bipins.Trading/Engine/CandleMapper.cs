using Bipins.Trading.Domain;
using IndCandle = Bipins.Trading.Indicators.Models.Candle;

namespace Bipins.Trading.Engine;

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

using Bipins.Trading.Domain;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Strategies;

internal static class CandleHelper
{
    public static Bipins.Trading.Indicators.Models.Candle ToIndicator(Bipins.Trading.Domain.Candle d) =>
        new(d.Time, d.Open, d.High, d.Low, d.Close, d.Volume);
}

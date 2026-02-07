using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Strategies;

internal static class CandleHelper
{
    public static Bipins.AI.Trading.Indicators.Models.Candle ToIndicator(Bipins.AI.Trading.Domain.Candle d) =>
        new(d.Time, d.Open, d.High, d.Low, d.Close, d.Volume);
}

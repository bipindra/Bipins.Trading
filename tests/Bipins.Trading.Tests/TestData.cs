using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Tests;

/// <summary>
/// Shared test candle data (known values for indicator verification).
/// </summary>
public static class TestData
{
    /// <summary>
    /// Sample 20 candles: simple uptrend with known OHLCV. Used for SMA/EMA/RSI known-value checks.
    /// </summary>
    public static IReadOnlyList<Candle> SampleCandles20()
    {
        var list = new List<Candle>();
        var baseTime = new DateTime(2024, 1, 1, 9, 30, 0);
        decimal[] closes = { 44, 44.34m, 44.09m, 43.61m, 44.33m, 44.83m, 45.10m, 45.42m, 45.84m, 46.08m, 45.89m, 46.03m, 45.61m, 46.28m, 46.28m, 46.00m, 46.03m, 46.41m, 46.22m, 45.64m };
        for (int i = 0; i < closes.Length; i++)
        {
            decimal c = closes[i];
            decimal h = c + 0.5m;
            decimal l = c - 0.5m;
            decimal o = i > 0 ? closes[i - 1] : c;
            list.Add(new Candle(baseTime.AddMinutes(i * 5), o, h, l, c, 1000 + i * 100));
        }
        return list;
    }

    public static Candle[] SampleCandles20Array()
    {
        var list = SampleCandles20();
        var arr = new Candle[list.Count];
        for (int i = 0; i < list.Count; i++)
            arr[i] = list[i];
        return arr;
    }
}

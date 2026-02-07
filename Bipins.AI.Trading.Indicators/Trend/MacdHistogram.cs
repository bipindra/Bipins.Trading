using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// MACD Histogram. Returns only the histogram component (MACD - Signal) of MACD.
/// Reference: Gerald Appel; same as MACD.
/// </summary>
public sealed class MacdHistogram : IndicatorBase<SingleValueResult>
{
    private readonly Macd _macd;

    /// <summary>
    /// MACD Histogram.
    /// </summary>
    /// <param name="fastPeriod">Fast EMA period (default 12).</param>
    /// <param name="slowPeriod">Slow EMA period (default 26).</param>
    /// <param name="signalPeriod">Signal period (default 9).</param>
    public MacdHistogram(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        : base("MACD Histogram", "MACD line minus Signal line.", slowPeriod + signalPeriod)
    {
        _macd = new Macd(fastPeriod, slowPeriod, signalPeriod);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _macd.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var r = _macd.Update(candle);
        if (!r.IsValid)
            return SingleValueResult.Invalid;
        return new SingleValueResult(r.GetValue(2), true);
    }
}

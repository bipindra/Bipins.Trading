using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.MovingAverages;

namespace Bipins.Trading.Indicators.Oscillators;

/// <summary>
/// Price Oscillator. SMA(Close, short) - SMA(Close, long).
/// Reference: Standard.
/// </summary>
public sealed class PriceOscillator : IndicatorBase<SingleValueResult>
{
    private readonly Sma _shortMa;
    private readonly Sma _longMa;

    /// <summary>
    /// Price Oscillator.
    /// </summary>
    /// <param name="shortPeriod">Short period (default 12).</param>
    /// <param name="longPeriod">Long period (default 26).</param>
    public PriceOscillator(int shortPeriod = 12, int longPeriod = 26)
        : base("Price Oscillator", "Short SMA - Long SMA of close.", longPeriod)
    {
        _shortMa = new Sma(shortPeriod);
        _longMa = new Sma(longPeriod);
        AddParameter("Short", shortPeriod.ToString(), "Short period");
        AddParameter("Long", longPeriod.ToString(), "Long period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _shortMa.Reset();
        _longMa.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var s = _shortMa.Update(candle);
        var l = _longMa.Update(candle);
        if (!s.IsValid || !l.IsValid)
            return SingleValueResult.Invalid;
        return new SingleValueResult(s.Value - l.Value, true);
    }
}

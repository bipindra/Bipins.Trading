using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Volatility;

/// <summary>
/// Volatility Ratio. ATR(short) / ATR(long). Measures short-term vs long-term volatility.
/// Reference: Standard.
/// </summary>
public sealed class VolatilityRatio : IndicatorBase<SingleValueResult>
{
    private readonly Atr _atrShort;
    private readonly Atr _atrLong;

    /// <summary>
    /// Volatility Ratio.
    /// </summary>
    /// <param name="shortPeriod">Short ATR period (default 10).</param>
    /// <param name="longPeriod">Long ATR period (default 20).</param>
    public VolatilityRatio(int shortPeriod = 10, int longPeriod = 20)
        : base("Volatility Ratio", "ATR short / ATR long.", longPeriod)
    {
        _atrShort = new Atr(shortPeriod);
        _atrLong = new Atr(longPeriod);
        AddParameter("ShortPeriod", shortPeriod.ToString(), "Short ATR period");
        AddParameter("LongPeriod", longPeriod.ToString(), "Long ATR period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _atrShort.Reset();
        _atrLong.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var s = _atrShort.Update(candle);
        var l = _atrLong.Update(candle);
        if (!s.IsValid || !l.IsValid)
            return SingleValueResult.Invalid;
        double vr = l.Value < 1e-20 ? 1 : s.Value / l.Value;
        return new SingleValueResult(vr, true);
    }
}

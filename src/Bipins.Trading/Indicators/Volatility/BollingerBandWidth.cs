using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Bollinger Band Width. (Upper - Lower) / Middle * 100. Measures volatility squeeze.
/// Reference: John Bollinger; Investopedia.
/// </summary>
public sealed class BollingerBandWidth : IndicatorBase<SingleValueResult>
{
    private readonly BollingerBands _bb;

    /// <summary>
    /// Bollinger Band Width.
    /// </summary>
    /// <param name="period">Bollinger period (default 20).</param>
    /// <param name="stdDevMultiplier">StdDev multiplier (default 2).</param>
    public BollingerBandWidth(int period = 20, double stdDevMultiplier = 2)
        : base("BB Width", "Bollinger Band Width. (Upper-Lower)/Middle.", period)
    {
        _bb = new BollingerBands(period, stdDevMultiplier);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _bb.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var r = _bb.Update(candle);
        if (!r.IsValid)
            return SingleValueResult.Invalid;
        double width = Math.Abs(r.Middle) < 1e-20 ? 0 : 100 * (r.Upper - r.Lower) / r.Middle;
        return new SingleValueResult(width, true);
    }
}

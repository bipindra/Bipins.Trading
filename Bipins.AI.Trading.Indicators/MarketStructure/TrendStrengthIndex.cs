using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MarketStructure;

/// <summary>
/// Trend Strength Index. Measures how much price is trending vs ranging (e.g. based on linear regression R-squared or ADX-like).
/// Simplified: 100 * |slope| / (std dev of close) over period.
/// Reference: Various.
/// </summary>
public sealed class TrendStrengthIndex : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Trend Strength Index.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public TrendStrengthIndex(int period = 14)
        : base("Trend Strength", "Trend strength 0-100.", period)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _closeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        _closeBuffer.Add((double)candle.Close);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        Span<double> y = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            y[i] = _closeBuffer[i];
        var (slope, _) = SpanMath.LinearRegression(y);
        double sd = SpanMath.StdDev(y, sample: true);
        if (sd < 1e-20) return new SingleValueResult(0, true);
        double tsi = Math.Min(100, 100 * Math.Abs(slope) / sd);
        return new SingleValueResult(tsi, true);
    }
}

using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// Z-Score Indicator. (Close - Mean(Close)) / StdDev(Close) over period. Standard score.
/// Reference: Standard statistical measure.
/// </summary>
public sealed class ZScoreIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Z-Score.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public ZScoreIndicator(int period = 20)
        : base("Z-Score", "Z-Score of close vs period mean.", period)
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
        double c = (double)candle.Close;
        _closeBuffer.Add(c);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        Span<double> span = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            span[i] = _closeBuffer[i];
        double mean = SpanMath.Mean(span);
        double sd = SpanMath.StdDev(span, sample: true);
        if (sd < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult((c - mean) / sd, true);
    }
}

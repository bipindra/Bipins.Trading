using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Volatility;

/// <summary>
/// Standard Deviation of closing price over period.
/// Reference: Standard statistical measure.
/// </summary>
public sealed class StandardDeviation : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Standard Deviation.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="sample">Use sample (n-1) variance if true.</param>
    public StandardDeviation(int period = 20, bool sample = false)
        : base("StdDev", "Standard Deviation of close over period.", period)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
        AddParameter("Sample", sample.ToString(), "Sample variance");
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
        Span<double> span = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            span[i] = _closeBuffer[i];
        double sd = SpanMath.StdDev(span, sample: true);
        return new SingleValueResult(sd, true);
    }
}

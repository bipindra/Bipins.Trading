using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Variance of closing price over period (population or sample).
/// Reference: Standard statistics.
/// </summary>
public sealed class VarianceIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Variance.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="sample">Use sample variance (n-1) if true.</param>
    public VarianceIndicator(int period = 20, bool sample = false)
        : base("Variance", "Variance of close over period.", period)
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
        return new SingleValueResult(SpanMath.Variance(span, sampleVariance: true), true);
    }
}

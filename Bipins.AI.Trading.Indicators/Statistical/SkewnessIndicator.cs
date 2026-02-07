using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Statistical;

/// <summary>
/// Skewness of closing price over period. Third standardized moment.
/// Reference: Standard statistics.
/// </summary>
public sealed class SkewnessIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Skewness.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public SkewnessIndicator(int period = 20)
        : base("Skewness", "Skewness of close over period.", period)
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
        double mean = _closeBuffer.Sum() / Period;
        double sum2 = 0, sum3 = 0;
        for (int i = 0; i < Period; i++)
        {
            double d = _closeBuffer[i] - mean;
            sum2 += d * d;
            sum3 += d * d * d;
        }
        double std = Math.Sqrt(sum2 / Period);
        if (std < 1e-20) return new SingleValueResult(0, true);
        double skew = (sum3 / Period) / (std * std * std);
        return new SingleValueResult(skew, true);
    }
}

using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Statistical;

/// <summary>
/// Kurtosis of closing price over period. Fourth standardized moment (excess kurtosis = 0 for normal).
/// Reference: Standard statistics.
/// </summary>
public sealed class KurtosisIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Kurtosis (excess).
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public KurtosisIndicator(int period = 20)
        : base("Kurtosis", "Excess kurtosis of close over period.", period)
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
        double sum2 = 0, sum4 = 0;
        for (int i = 0; i < Period; i++)
        {
            double d = _closeBuffer[i] - mean;
            sum2 += d * d;
            sum4 += d * d * d * d;
        }
        double var = sum2 / Period;
        if (var < 1e-20) return new SingleValueResult(0, true);
        double kurt = (sum4 / Period) / (var * var) - 3;
        return new SingleValueResult(kurt, true);
    }
}

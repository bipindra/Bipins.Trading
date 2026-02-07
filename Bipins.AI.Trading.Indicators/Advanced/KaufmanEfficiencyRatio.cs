using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// Kaufman Efficiency Ratio (ER). |Close - Close_n| / Sum(|Close_i - Close_{i-1}|). Measures trend efficiency.
/// Reference: Perry Kaufman; "Trading Systems and Methods".
/// </summary>
public sealed class KaufmanEfficiencyRatio : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Kaufman Efficiency Ratio.
    /// </summary>
    /// <param name="period">Period (default 10).</param>
    public KaufmanEfficiencyRatio(int period = 10)
        : base("Kaufman ER", "Kaufman Efficiency Ratio. Trend efficiency 0-1.", period + 1)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period + 1);
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
        double change = Math.Abs(_closeBuffer[0] - _closeBuffer[Period]);
        double volatility = 0;
        for (int i = 0; i < Period; i++)
            volatility += Math.Abs(_closeBuffer[i] - _closeBuffer[i + 1]);
        if (volatility < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult(Math.Min(1, change / volatility), true);
    }
}

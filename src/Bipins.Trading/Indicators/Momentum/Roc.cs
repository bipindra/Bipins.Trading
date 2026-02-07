using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Rate of Change (ROC). (Close - Close_n) / Close_n * 100. Momentum over N periods.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Roc : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Rate of Change.
    /// </summary>
    /// <param name="period">Lookback period (default 12).</param>
    public Roc(int period = 12)
        : base("ROC", "Rate of Change. Percentage change over N periods.", period + 1)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Lookback period");
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
        double prev = _closeBuffer[0];
        if (Math.Abs(prev) < 1e-20) prev = 1e-20;
        double roc = (c - prev) / prev * 100;
        return new SingleValueResult(roc, true);
    }
}

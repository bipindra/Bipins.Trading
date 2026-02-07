using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Statistical;

/// <summary>
/// Mean (average) of closing price over period.
/// Reference: Standard statistics.
/// </summary>
public sealed class MeanIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Mean.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public MeanIndicator(int period = 20)
        : base("Mean", "Arithmetic mean of close.", period)
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
        return new SingleValueResult(_closeBuffer.Sum() / Period, true);
    }
}

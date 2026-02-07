using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Momentum;

/// <summary>
/// Williams %R. -100 * (Highest High - Close) / (Highest High - Lowest Low) over period. Range -100 to 0.
/// Reference: Larry Williams; Investopedia.
/// </summary>
public sealed class WilliamsR : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Williams %R.
    /// </summary>
    /// <param name="period">Lookback period (default 14).</param>
    public WilliamsR(int period = 14)
        : base("Williams %R", "Williams Percent Range. Momentum -100 to 0.", period)
    {
        Period = period;
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        if (!_highBuffer.IsFull)
            return SingleValueResult.Invalid;
        double hh = double.MinValue, ll = double.MaxValue;
        for (int i = 0; i < Period; i++)
        {
            if (_highBuffer[i] > hh) hh = _highBuffer[i];
            if (_lowBuffer[i] < ll) ll = _lowBuffer[i];
        }
        double range = hh - ll;
        if (range < 1e-20) return new SingleValueResult(-50, true);
        double wr = -100 * (hh - (double)candle.Close) / range;
        return new SingleValueResult(Math.Clamp(wr, -100, 0), true);
    }
}

using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Donchian Channels. Upper = highest high, Lower = lowest low, Middle = (Upper+Lower)/2 over period.
/// Reference: Richard Donchian; Investopedia.
/// </summary>
public sealed class DonchianChannels : IndicatorBase<BandResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Donchian Channels.
    /// </summary>
    /// <param name="period">Lookback period (default 20).</param>
    public DonchianChannels(int period = 20)
        : base("Donchian", "Donchian Channels. High-low range.", period)
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
    protected override BandResult ComputeNext(Candle candle)
    {
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        if (!_highBuffer.IsFull)
            return BandResult.Invalid;
        double upper = double.MinValue, lower = double.MaxValue;
        for (int i = 0; i < Period; i++)
        {
            if (_highBuffer[i] > upper) upper = _highBuffer[i];
            if (_lowBuffer[i] < lower) lower = _lowBuffer[i];
        }
        double middle = (upper + lower) / 2;
        return new BandResult(upper, middle, lower, true);
    }
}

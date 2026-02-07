using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Detrended Price Oscillator (DPO). Price minus displaced moving average to remove trend. DPO = Close - SMA(Close, period) displaced back by period/2+1.
/// Reference: William Blau; Investopedia.
/// </summary>
public sealed class Dpo : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly int _displace;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Detrended Price Oscillator.
    /// </summary>
    /// <param name="period">SMA period (default 20).</param>
    public Dpo(int period = 20)
        : base("DPO", "Detrended Price Oscillator. Price minus displaced SMA.", period + (period / 2 + 1))
    {
        Period = period;
        _displace = period / 2 + 1;
        _closeBuffer = new RingBufferDouble(period + _displace);
        AddParameter("Period", period.ToString(), "SMA period");
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
        double sma = 0;
        for (int i = _displace; i < _displace + Period; i++)
            sma += _closeBuffer[i];
        sma /= Period;
        double dpo = c - sma;
        return new SingleValueResult(dpo, true);
    }
}

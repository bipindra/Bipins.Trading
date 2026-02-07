using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Zero Lag Moving Average (ZLMA). Eliminates lag by combining EMA with lag correction.
/// Formula: Lag = (period-1)/2; ZLMA = EMA(2*Close - Close_lag) or EMA(2*EMA(Close) - EMA(Close, lag)).
/// Common: ZLMA = EMA(2*Close - EMA(Close, period)).
/// Reference: John Ehlers; TASC.
/// </summary>
public sealed class Zlma : IndicatorBase<SingleValueResult>
{
    private readonly Ema _emaClose;
    private readonly Ema _emaRaw;
    private readonly int _lag;
    private readonly RingBufferDouble _closeBuffer;
    private bool _lagFilled;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Zero Lag Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Zlma(int period = 14)
        : base("ZLMA", "Zero Lag Moving Average. Lag-corrected EMA.", period * 2)
    {
        Period = period;
        _lag = (period - 1) / 2;
        _emaClose = new Ema(period);
        _emaRaw = new Ema(period);
        _closeBuffer = new RingBufferDouble(_lag + 1);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _emaClose.Reset();
        _emaRaw.Reset();
        _closeBuffer.Clear();
        _lagFilled = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        _closeBuffer.Add(c);
        if (!_closeBuffer.IsFull)
            _lagFilled = false;
        else
            _lagFilled = true;
        var emaC = _emaClose.Update(candle);
        if (!emaC.IsValid)
            return SingleValueResult.Invalid;
        double laggedClose = _lagFilled ? _closeBuffer[0] : c;
        double raw = 2 * c - laggedClose;
        var emaRaw = _emaRaw.Update(new Candle(candle.Time, (decimal)raw, (decimal)raw, (decimal)raw, (decimal)raw, candle.Volume));
        if (!emaRaw.IsValid)
            return SingleValueResult.Invalid;
        return new SingleValueResult(emaRaw.Value, true);
    }
}

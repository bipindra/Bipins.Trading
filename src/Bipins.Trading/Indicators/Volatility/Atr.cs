using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Average True Range (ATR). Smoothed (RMA) true range over period.
/// Formula: TR = max(H-L, |H-PrevC|, |L-PrevC|); ATR = RMA(TR).
/// Reference: J. Welles Wilder; "New Concepts in Technical Trading Systems".
/// </summary>
public sealed class Atr : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _trBuffer;
    private double _prevClose;
    private bool _havePrev;
    private double _atr;
    private bool _atrInitialized;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Average True Range.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public Atr(int period = 14)
        : base("ATR", "Average True Range. Volatility measure.", period)
    {
        Period = period;
        _trBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _trBuffer.Clear();
        _havePrev = false;
        _atrInitialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double tr = _havePrev ? (double)Candle.TrueRange((decimal)h, (decimal)l, (decimal)_prevClose) : h - l;
        _havePrev = true;
        _prevClose = c;
        _trBuffer.Add(tr);
        if (!_trBuffer.IsFull)
            return SingleValueResult.Invalid;
        if (!_atrInitialized)
        {
            _atr = _trBuffer.Sum() / Period;
            _atrInitialized = true;
        }
        else
        {
            _atr = (_atr * (Period - 1) + tr) / Period;
        }
        return new SingleValueResult(_atr, true);
    }
}

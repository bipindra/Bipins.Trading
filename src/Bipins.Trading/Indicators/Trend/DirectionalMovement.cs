using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Trend;

/// <summary>
/// Directional Movement Index (+DI and -DI). Plus DI and Minus DI from Wilder's system.
/// Reference: J. Welles Wilder; used with ADX.
/// </summary>
public sealed class DirectionalMovement : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _trBuffer;
    private readonly RingBufferDouble _plusDmBuffer;
    private readonly RingBufferDouble _minusDmBuffer;
    private double _prevHigh, _prevLow, _prevClose;
    private bool _havePrev;
    private double _atr, _plusDi, _minusDi;
    private bool _smoothedInitialized;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Directional Movement (+DI, -DI).
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public DirectionalMovement(int period = 14)
        : base("DI", "Directional Movement Index +DI and -DI.", period)
    {
        Period = period;
        _trBuffer = new RingBufferDouble(period);
        _plusDmBuffer = new RingBufferDouble(period);
        _minusDmBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _trBuffer.Clear();
        _plusDmBuffer.Clear();
        _minusDmBuffer.Clear();
        _havePrev = false;
        _smoothedInitialized = false;
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double tr = _havePrev ? (double)Candle.TrueRange((decimal)h, (decimal)l, (decimal)_prevClose) : h - l;
        double plusDm = 0, minusDm = 0;
        if (_havePrev)
        {
            double upMove = h - _prevHigh;
            double downMove = _prevLow - l;
            if (upMove > downMove && upMove > 0) plusDm = upMove;
            if (downMove > upMove && downMove > 0) minusDm = downMove;
        }
        _prevHigh = h; _prevLow = l; _prevClose = c;
        _havePrev = true;
        _trBuffer.Add(tr);
        _plusDmBuffer.Add(plusDm);
        _minusDmBuffer.Add(minusDm);
        if (!_trBuffer.IsFull)
            return MultiValueResult.Invalid();
        if (!_smoothedInitialized)
        {
            _atr = _trBuffer.Sum() / Period;
            double sumTr = _trBuffer.Sum();
            _plusDi = sumTr < 1e-20 ? 0 : 100 * _plusDmBuffer.Sum() / sumTr;
            _minusDi = sumTr < 1e-20 ? 0 : 100 * _minusDmBuffer.Sum() / sumTr;
            _smoothedInitialized = true;
        }
        else
        {
            _atr = (_atr * (Period - 1) + tr) / Period;
            double rmaPlus = (_plusDi / 100 * _atr * (Period - 1) + plusDm) / Period;
            double rmaMinus = (_minusDi / 100 * _atr * (Period - 1) + minusDm) / Period;
            _plusDi = _atr < 1e-20 ? 0 : 100 * rmaPlus / _atr;
            _minusDi = _atr < 1e-20 ? 0 : 100 * rmaMinus / _atr;
        }
        return new MultiValueResult(_plusDi, _minusDi, true);
    }
}

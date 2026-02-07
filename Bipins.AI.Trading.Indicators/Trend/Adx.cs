using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Average Directional Index (ADX). Measures trend strength. Uses +DM, -DM, TR smoothed (RMA), then DX smoothed.
/// Reference: J. Welles Wilder; "New Concepts in Technical Trading Systems".
/// </summary>
public sealed class Adx : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _trBuffer;
    private readonly RingBufferDouble _plusDmBuffer;
    private readonly RingBufferDouble _minusDmBuffer;
    private double _prevHigh, _prevLow, _prevClose;
    private bool _havePrev;
    private double _atr, _plusDi, _minusDi, _adx;
    private bool _smoothedInitialized;
    private int _dxCount;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Average Directional Index.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public Adx(int period = 14)
        : base("ADX", "Average Directional Index. Trend strength 0-100.", period * 2)
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
        _dxCount = 0;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
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
        _prevHigh = h;
        _prevLow = l;
        _prevClose = c;
        _havePrev = true;
        _trBuffer.Add(tr);
        _plusDmBuffer.Add(plusDm);
        _minusDmBuffer.Add(minusDm);
        if (!_trBuffer.IsFull)
            return SingleValueResult.Invalid;
        if (!_smoothedInitialized)
        {
            _atr = _trBuffer.Sum() / Period;
            _plusDi = _plusDmBuffer.Sum() < 1e-20 ? 0 : 100 * _plusDmBuffer.Sum() / _trBuffer.Sum();
            _minusDi = _minusDmBuffer.Sum() < 1e-20 ? 0 : 100 * _minusDmBuffer.Sum() / _trBuffer.Sum();
            _smoothedInitialized = true;
        }
        else
        {
            _atr = (_atr * (Period - 1) + tr) / Period;
            _plusDi = _atr < 1e-20 ? 0 : 100 * ((_plusDi / 100 * _atr * (Period - 1) + plusDm) / Period) / _atr;
            _minusDi = _atr < 1e-20 ? 0 : 100 * ((_minusDi / 100 * _atr * (Period - 1) + minusDm) / Period) / _atr;
        }
        double diSum = _plusDi + _minusDi;
        if (diSum < 1e-20) return new SingleValueResult(_adx, true);
        double dx = 100 * Math.Abs(_plusDi - _minusDi) / diSum;
        if (_dxCount < Period)
        {
            _adx = _dxCount == 0 ? dx : (_adx * _dxCount + dx) / (_dxCount + 1);
            _dxCount++;
        }
        else
        {
            _adx = (_adx * (Period - 1) + dx) / Period;
        }
        return new SingleValueResult(_adx, true);
    }
}

using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Ultimate Oscillator. Weighted sum of buying pressure / true range over 7, 14, 28 periods.
/// BP = Close - Min(Low, PrevClose), TR = True Range. Reference: Larry Williams.
/// </summary>
public sealed class UltimateOscillator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _bpBuffer7;
    private readonly RingBufferDouble _trBuffer7;
    private readonly RingBufferDouble _bpBuffer14;
    private readonly RingBufferDouble _trBuffer14;
    private readonly RingBufferDouble _bpBuffer28;
    private readonly RingBufferDouble _trBuffer28;
    private double _prevClose;
    private bool _havePrev;

    /// <summary>
    /// Ultimate Oscillator.
    /// </summary>
    /// <param name="period1">Short period (default 7).</param>
    /// <param name="period2">Medium period (default 14).</param>
    /// <param name="period3">Long period (default 28).</param>
    public UltimateOscillator(int period1 = 7, int period2 = 14, int period3 = 28)
        : base("Ultimate Oscillator", "Ultimate Oscillator. Multi-timeframe momentum.", period3)
    {
        _bpBuffer7 = new RingBufferDouble(period1);
        _trBuffer7 = new RingBufferDouble(period1);
        _bpBuffer14 = new RingBufferDouble(period2);
        _trBuffer14 = new RingBufferDouble(period2);
        _bpBuffer28 = new RingBufferDouble(period3);
        _trBuffer28 = new RingBufferDouble(period3);
        AddParameter("Period1", period1.ToString(), "Short period");
        AddParameter("Period2", period2.ToString(), "Medium period");
        AddParameter("Period3", period3.ToString(), "Long period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _bpBuffer7.Clear();
        _trBuffer7.Clear();
        _bpBuffer14.Clear();
        _trBuffer14.Clear();
        _bpBuffer28.Clear();
        _trBuffer28.Clear();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double prev = _havePrev ? _prevClose : l;
        _havePrev = true;
        _prevClose = c;
        double bp = c - Math.Min(l, prev);
        double tr = (double)Candle.TrueRange((decimal)h, (decimal)l, (decimal)prev);
        _bpBuffer7.Add(bp);
        _trBuffer7.Add(tr);
        _bpBuffer14.Add(bp);
        _trBuffer14.Add(tr);
        _bpBuffer28.Add(bp);
        _trBuffer28.Add(tr);
        if (!_bpBuffer28.IsFull)
            return SingleValueResult.Invalid;
        double avg7 = _trBuffer7.Sum() < 1e-20 ? 0 : _bpBuffer7.Sum() / _trBuffer7.Sum();
        double avg14 = _trBuffer14.Sum() < 1e-20 ? 0 : _bpBuffer14.Sum() / _trBuffer14.Sum();
        double avg28 = _trBuffer28.Sum() < 1e-20 ? 0 : _bpBuffer28.Sum() / _trBuffer28.Sum();
        double uo = 100 * (4 * avg7 + 2 * avg14 + avg28) / 7;
        return new SingleValueResult(Math.Clamp(uo, 0, 100), true);
    }
}

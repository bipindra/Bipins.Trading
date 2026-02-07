using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Vortex Indicator (VI+ and VI-). VM+ = |High - PrevLow|, VM- = |Low - PrevHigh|; VI = sum(VM)/sum(TR) over period.
/// Reference: Etienne Botes, Douglas Siepman; Investopedia.
/// </summary>
public sealed class VortexIndicator : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _trBuffer;
    private readonly RingBufferDouble _vmPlusBuffer;
    private readonly RingBufferDouble _vmMinusBuffer;
    private double _prevHigh, _prevLow, _prevClose;
    private bool _havePrev;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Vortex Indicator (VI+, VI-).
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public VortexIndicator(int period = 14)
        : base("Vortex", "Vortex Indicator VI+ and VI-.", period)
    {
        Period = period;
        _trBuffer = new RingBufferDouble(period);
        _vmPlusBuffer = new RingBufferDouble(period);
        _vmMinusBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _trBuffer.Clear();
        _vmPlusBuffer.Clear();
        _vmMinusBuffer.Clear();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double tr = _havePrev ? (double)Candle.TrueRange((decimal)h, (decimal)l, (decimal)_prevClose) : h - l;
        double vmPlus = _havePrev ? Math.Abs(h - _prevLow) : 0;
        double vmMinus = _havePrev ? Math.Abs(l - _prevHigh) : 0;
        _prevHigh = h;
        _prevLow = l;
        _prevClose = c;
        _havePrev = true;
        _trBuffer.Add(tr);
        _vmPlusBuffer.Add(vmPlus);
        _vmMinusBuffer.Add(vmMinus);
        if (!_trBuffer.IsFull)
            return MultiValueResult.Invalid();
        double sumTr = _trBuffer.Sum();
        if (sumTr < 1e-20) sumTr = 1e-20;
        double viPlus = 100 * _vmPlusBuffer.Sum() / sumTr;
        double viMinus = 100 * _vmMinusBuffer.Sum() / sumTr;
        return new MultiValueResult(viPlus, viMinus, true);
    }
}

using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// On Balance Volume (OBV). Cumulative volume: add volume when close greater than previous close, subtract when close less than previous close.
/// Reference: Joe Granville; Investopedia.
/// </summary>
public sealed class Obv : IndicatorBase<SingleValueResult>
{
    private double _obv;
    private double _prevClose;
    private bool _havePrev;

    /// <summary>
    /// On Balance Volume.
    /// </summary>
    public Obv()
        : base("OBV", "On Balance Volume. Cumulative volume by direction.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _obv = 0;
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        if (_havePrev)
        {
            if (c > _prevClose) _obv += v;
            else if (c < _prevClose) _obv -= v;
        }
        _prevClose = c;
        _havePrev = true;
        return new SingleValueResult(_obv, true);
    }
}

using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Volume;

/// <summary>
/// Positive Volume Index (PVI). Cumulative: add percent price change only when volume is greater than previous volume. Starts at 1000.
/// Reference: Norman Fosback; Investopedia.
/// </summary>
public sealed class Pvi : IndicatorBase<SingleValueResult>
{
    private double _pvi;
    private double _prevClose;
    private double _prevVolume;
    private bool _havePrev;

    /// <summary>
    /// Positive Volume Index.
    /// </summary>
    public Pvi()
        : base("PVI", "Positive Volume Index. Cumulative on up-volume days.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _pvi = 1000;
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        if (_havePrev && v > _prevVolume && _prevClose > 0)
            _pvi += _pvi * (c - _prevClose) / _prevClose;
        _prevClose = c;
        _prevVolume = v;
        _havePrev = true;
        return new SingleValueResult(_pvi, true);
    }
}

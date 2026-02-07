using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Volume;

/// <summary>
/// Negative Volume Index (NVI). Cumulative: add percent price change only when volume is less than previous volume. Starts at 1000.
/// Reference: Norman Fosback; Investopedia.
/// </summary>
public sealed class Nvi : IndicatorBase<SingleValueResult>
{
    private double _nvi;
    private double _prevClose;
    private double _prevVolume;
    private bool _havePrev;

    /// <summary>
    /// Negative Volume Index.
    /// </summary>
    public Nvi()
        : base("NVI", "Negative Volume Index. Cumulative on down-volume days.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _nvi = 1000;
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        if (_havePrev && v < _prevVolume && _prevClose > 0)
            _nvi += _nvi * (c - _prevClose) / _prevClose;
        _prevClose = c;
        _prevVolume = v;
        _havePrev = true;
        return new SingleValueResult(_nvi, true);
    }
}

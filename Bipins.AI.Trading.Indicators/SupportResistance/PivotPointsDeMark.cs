using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// DeMark Pivot Points. If Close less than Open then X = H+2*L+C, else if Close greater than Open then X = 2*H+L+C, else X = H+L+2*C. P = X/4, R1 = X/2 - L, S1 = X/2 - H.
/// Reference: Tom DeMark; Investopedia.
/// </summary>
public sealed class PivotPointsDeMark : IndicatorBase<MultiValueResult>
{
    private double _prevH, _prevL, _prevC, _prevO;
    private bool _havePrev;

    /// <summary>
    /// DeMark Pivot Points.
    /// </summary>
    public PivotPointsDeMark()
        : base("Pivot DeMark", "DeMark pivot points.", 2)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double o = (double)candle.Open;
        if (!_havePrev)
        {
            _prevH = h; _prevL = l; _prevC = c; _prevO = o;
            _havePrev = true;
            return MultiValueResult.Invalid();
        }
        double x;
        if (_prevC < _prevO) x = _prevH + 2 * _prevL + _prevC;
        else if (_prevC > _prevO) x = 2 * _prevH + _prevL + _prevC;
        else x = _prevH + _prevL + 2 * _prevC;
        double p = x / 4;
        double r1 = x / 2 - _prevL;
        double s1 = x / 2 - _prevH;
        _prevH = h; _prevL = l; _prevC = c; _prevO = o;
        return new MultiValueResult(p, r1, s1, true);
    }
}

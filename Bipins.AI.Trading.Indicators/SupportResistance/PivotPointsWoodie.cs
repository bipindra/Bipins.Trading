using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// Woodie Pivot Points. P = (H+L+2*C)/4, R1 = 2*P-L, R2 = P+H-L, S1 = 2*P-H, S2 = P-H+L.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class PivotPointsWoodie : IndicatorBase<MultiValueResult>
{
    private double _prevH, _prevL, _prevC;
    private bool _havePrev;

    /// <summary>
    /// Woodie Pivot Points.
    /// </summary>
    public PivotPointsWoodie()
        : base("Pivot Woodie", "Woodie pivot points.", 2)
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
        if (!_havePrev)
        {
            _prevH = h; _prevL = l; _prevC = c;
            _havePrev = true;
            return MultiValueResult.Invalid();
        }
        double p = (_prevH + _prevL + 2 * _prevC) / 4;
        double r1 = 2 * p - _prevL;
        double r2 = p + _prevH - _prevL;
        double s1 = 2 * p - _prevH;
        double s2 = p - _prevH + _prevL;
        _prevH = h; _prevL = l; _prevC = c;
        return new MultiValueResult(p, r1, r2, s1, s2, true);
    }
}

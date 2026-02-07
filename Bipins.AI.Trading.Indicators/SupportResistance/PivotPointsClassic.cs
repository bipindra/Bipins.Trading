using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// Classic Pivot Points. P = (H+L+C)/3, R1 = 2*P-L, R2 = P+(H-L), S1 = 2*P-H, S2 = P-(H-L). Uses previous bar H,L,C.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class PivotPointsClassic : IndicatorBase<MultiValueResult>
{
    private double _prevH, _prevL, _prevC;
    private bool _havePrev;

    /// <summary>
    /// Classic Pivot Points (P, R1, R2, S1, S2).
    /// </summary>
    public PivotPointsClassic()
        : base("Pivot Classic", "Classic pivot points from previous bar.", 2)
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
        double p = (_prevH + _prevL + _prevC) / 3;
        double r1 = 2 * p - _prevL;
        double r2 = p + (_prevH - _prevL);
        double s1 = 2 * p - _prevH;
        double s2 = p - (_prevH - _prevL);
        _prevH = h; _prevL = l; _prevC = c;
        return new MultiValueResult(p, r1, r2, s1, s2, true);
    }
}

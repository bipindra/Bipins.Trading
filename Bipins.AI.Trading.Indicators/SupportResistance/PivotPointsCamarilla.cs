using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// Camarilla Pivot Points. Uses specific coefficients for R1-R4 and S1-S4. P = (H+L+C)/3.
/// Reference: Nick Stott; Investopedia.
/// </summary>
public sealed class PivotPointsCamarilla : IndicatorBase<MultiValueResult>
{
    private double _prevH, _prevL, _prevC;
    private bool _havePrev;

    /// <summary>
    /// Camarilla Pivot Points (P, R1-R2, S1-S2 simplified).
    /// </summary>
    public PivotPointsCamarilla()
        : base("Pivot Camarilla", "Camarilla pivot points.", 2)
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
        double range = _prevH - _prevL;
        double p = (_prevH + _prevL + _prevC) / 3;
        double r1 = c + range * 1.1 / 12;
        double r2 = c + range * 1.1 / 6;
        double s1 = c - range * 1.1 / 12;
        double s2 = c - range * 1.1 / 6;
        _prevH = h; _prevL = l; _prevC = c;
        return new MultiValueResult(p, r1, r2, s1, s2, true);
    }
}

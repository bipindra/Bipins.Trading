using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.SupportResistance;

/// <summary>
/// Fibonacci Pivot Points. P = (H+L+C)/3, R1 = P + 0.382*(H-L), R2 = P + 0.618*(H-L), S1 = P - 0.382*(H-L), S2 = P - 0.618*(H-L).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class PivotPointsFibonacci : IndicatorBase<MultiValueResult>
{
    private double _prevH, _prevL, _prevC;
    private bool _havePrev;

    /// <summary>
    /// Fibonacci Pivot Points.
    /// </summary>
    public PivotPointsFibonacci()
        : base("Pivot Fibonacci", "Fibonacci pivot points.", 2)
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
        double range = _prevH - _prevL;
        double r1 = p + 0.382 * range;
        double r2 = p + 0.618 * range;
        double s1 = p - 0.382 * range;
        double s2 = p - 0.618 * range;
        _prevH = h; _prevL = l; _prevC = c;
        return new MultiValueResult(p, r1, r2, s1, s2, true);
    }
}

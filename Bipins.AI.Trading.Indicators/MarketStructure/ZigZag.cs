using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.MarketStructure;

/// <summary>
/// ZigZag. Filters price moves below threshold; outputs segment endpoints. Returns current ZigZag value (last pivot or current price).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class ZigZag : IndicatorBase<SingleValueResult>
{
    private readonly double _deviationPercent;
    private double _lastPivot;
    private double _lastPivotPrice;
    private bool _trendUp;
    private bool _initialized;

    /// <summary>Deviation threshold (percent).</summary>
    public double DeviationPercent => _deviationPercent;

    /// <summary>
    /// ZigZag.
    /// </summary>
    /// <param name="deviationPercent">Minimum move percent to form pivot (default 5).</param>
    public ZigZag(double deviationPercent = 5)
        : base("ZigZag", "ZigZag. Filters small moves.", 1)
    {
        _deviationPercent = deviationPercent;
        AddParameter("Deviation", deviationPercent.ToString(), "Min move percent");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _initialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        if (!_initialized)
        {
            _lastPivot = l;
            _lastPivotPrice = l;
            _trendUp = true;
            _initialized = true;
            return new SingleValueResult(c, true);
        }
        double thresh = _lastPivotPrice * (_deviationPercent / 100);
        if (_trendUp)
        {
            if (l < _lastPivot - thresh) { _trendUp = false; _lastPivot = h; _lastPivotPrice = _lastPivot; }
            else if (h > _lastPivot) _lastPivot = h;
        }
        else
        {
            if (h > _lastPivot + thresh) { _trendUp = true; _lastPivot = l; _lastPivotPrice = _lastPivot; }
            else if (l < _lastPivot) _lastPivot = l;
        }
        return new SingleValueResult(_lastPivot, true);
    }
}

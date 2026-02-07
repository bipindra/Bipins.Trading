using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.PriceTransform;

/// <summary>
/// Log Returns. ln(Close / PrevClose). First bar returns 0.
/// Reference: Standard finance; continuous compounding return.
/// </summary>
public sealed class LogReturns : IndicatorBase<SingleValueResult>
{
    private double _prevClose;
    private bool _havePrev;

    /// <summary>
    /// Log Returns.
    /// </summary>
    public LogReturns()
        : base("Log Returns", "Natural log of close ratio.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double logRet = _havePrev && _prevClose > 0 ? Math.Log(c / _prevClose) : 0;
        _prevClose = c;
        _havePrev = true;
        return new SingleValueResult(logRet, true);
    }
}

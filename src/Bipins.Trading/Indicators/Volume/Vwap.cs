using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// VWAP (Volume Weighted Average Price). Cumulative Sum(Typical Price * Volume) / Sum(Volume) from session start.
/// Reference: Standard; institutional benchmark.
/// </summary>
public sealed class Vwap : IndicatorBase<SingleValueResult>
{
    private double _cumTpV;
    private double _cumV;

    /// <summary>
    /// VWAP.
    /// </summary>
    public Vwap()
        : base("VWAP", "Volume Weighted Average Price. Session VWAP.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _cumTpV = 0;
        _cumV = 0;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        double v = (double)candle.Volume;
        _cumTpV += tp * v;
        _cumV += v;
        if (_cumV < 1e-20) return new SingleValueResult(tp, true);
        return new SingleValueResult(_cumTpV / _cumV, true);
    }
}

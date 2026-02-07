using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.PriceTransform;

/// <summary>
/// Heikin Ashi. Smoothed OHLC: HA_Close = (O+H+L+C)/4, HA_Open = prev HA_Close, HA_High = max(H, HA_Open, HA_Close), HA_Low = min(L, HA_Open, HA_Close).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class HeikinAshi : IndicatorBase<MultiValueResult>
{
    private double _prevHaClose;

    /// <summary>
    /// Heikin Ashi (HA Open, High, Low, Close).
    /// </summary>
    public HeikinAshi()
        : base("Heikin Ashi", "Heikin Ashi smoothed OHLC.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double o = (double)candle.Open;
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double haClose = (o + h + l + c) / 4;
        double haOpen = UpdateCount > 0 ? _prevHaClose : (o + c) / 2;
        double haHigh = Math.Max(h, Math.Max(haOpen, haClose));
        double haLow = Math.Min(l, Math.Min(haOpen, haClose));
        _prevHaClose = haClose;
        return new MultiValueResult(haOpen, haHigh, haLow, haClose, true);
    }
}

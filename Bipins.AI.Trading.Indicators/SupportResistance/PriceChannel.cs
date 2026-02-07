using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Volatility;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// Price Channel. Upper = highest high, Lower = lowest low, Middle = (Upper+Lower)/2 over period. Same as Donchian.
/// Reference: Richard Donchian; Investopedia.
/// </summary>
public sealed class PriceChannel : IndicatorBase<BandResult>
{
    private readonly DonchianChannels _donchian;

    /// <summary>
    /// Price Channel (Donchian).
    /// </summary>
    /// <param name="period">Lookback period (default 20).</param>
    public PriceChannel(int period = 20)
        : base("Price Channel", "Price Channel (Donchian). Upper, Middle, Lower.", period)
    {
        _donchian = new DonchianChannels(period);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _donchian.Reset();
    }

    /// <inheritdoc />
    protected override BandResult ComputeNext(Candle candle)
    {
        return _donchian.Update(candle);
    }
}

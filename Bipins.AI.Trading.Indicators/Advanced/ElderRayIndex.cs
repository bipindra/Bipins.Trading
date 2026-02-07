using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.MovingAverages;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// Elder Ray Index. Bull Power = High - EMA(Close), Bear Power = Low - EMA(Close).
/// Reference: Alexander Elder; Investopedia.
/// </summary>
public sealed class ElderRayIndex : IndicatorBase<MultiValueResult>
{
    private readonly Ema _ema;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Elder Ray Index (Bull Power, Bear Power).
    /// </summary>
    /// <param name="period">EMA period (default 13).</param>
    public ElderRayIndex(int period = 13)
        : base("Elder Ray", "Bull Power and Bear Power.", period)
    {
        Period = period;
        _ema = new Ema(period);
        AddParameter("Period", period.ToString(), "EMA period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema.Reset();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        var emaR = _ema.Update(candle);
        if (!emaR.IsValid)
            return MultiValueResult.Invalid();
        double bull = (double)candle.High - emaR.Value;
        double bear = (double)candle.Low - emaR.Value;
        return new MultiValueResult(bull, bear, true);
    }
}

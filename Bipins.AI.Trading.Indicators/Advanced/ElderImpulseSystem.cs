using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.MovingAverages;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// Elder Impulse System. Color based on EMA slope and price vs EMA: +1 bullish, -1 bearish, 0 neutral.
/// Reference: Alexander Elder; Investopedia.
/// </summary>
public sealed class ElderImpulseSystem : IndicatorBase<SingleValueResult>
{
    private readonly Ema _ema;
    private double _prevEma;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Elder Impulse System.
    /// </summary>
    /// <param name="period">EMA period (default 13).</param>
    public ElderImpulseSystem(int period = 13)
        : base("Elder Impulse", "Elder Impulse System. +1/-1/0.", period + 1)
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
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var emaR = _ema.Update(candle);
        if (!emaR.IsValid)
            return SingleValueResult.Invalid;
        double c = (double)candle.Close;
        double impulse = 0;
        if (UpdateCount > Period)
        {
            if (c > emaR.Value && emaR.Value > _prevEma) impulse = 1;
            else if (c < emaR.Value && emaR.Value < _prevEma) impulse = -1;
        }
        _prevEma = emaR.Value;
        return new SingleValueResult(impulse, true);
    }
}

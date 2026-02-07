using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.MovingAverages;

namespace Bipins.AI.Trading.Indicators.Oscillators;

/// <summary>
/// Percentage Price Oscillator (PPO). (EMA(short) - EMA(long)) / EMA(long) * 100.
/// Reference: Standard; like MACD in percentage.
/// </summary>
public sealed class Ppo : IndicatorBase<SingleValueResult>
{
    private readonly Ema _emaShort;
    private readonly Ema _emaLong;

    /// <summary>
    /// PPO.
    /// </summary>
    /// <param name="shortPeriod">Short EMA (default 12).</param>
    /// <param name="longPeriod">Long EMA (default 26).</param>
    public Ppo(int shortPeriod = 12, int longPeriod = 26)
        : base("PPO", "Percentage Price Oscillator.", longPeriod)
    {
        _emaShort = new Ema(shortPeriod);
        _emaLong = new Ema(longPeriod);
        AddParameter("Short", shortPeriod.ToString(), "Short EMA");
        AddParameter("Long", longPeriod.ToString(), "Long EMA");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _emaShort.Reset();
        _emaLong.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var s = _emaShort.Update(candle);
        var l = _emaLong.Update(candle);
        if (!s.IsValid || !l.IsValid)
            return SingleValueResult.Invalid;
        if (Math.Abs(l.Value) < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult(100 * (s.Value - l.Value) / l.Value, true);
    }
}

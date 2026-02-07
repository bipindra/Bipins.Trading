using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Aroon Oscillator. Aroon Up - Aroon Down. Range -100 to 100.
/// Reference: Tushar Chande; Investopedia.
/// </summary>
public sealed class AroonOscillator : IndicatorBase<SingleValueResult>
{
    private readonly Aroon _aroon;

    /// <summary>
    /// Aroon Oscillator.
    /// </summary>
    /// <param name="period">Lookback period (default 25).</param>
    public AroonOscillator(int period = 25)
        : base("Aroon Oscillator", "Aroon Up minus Aroon Down.", period + 1)
    {
        _aroon = new Aroon(period);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _aroon.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var r = _aroon.Update(candle);
        if (!r.IsValid)
            return SingleValueResult.Invalid;
        return new SingleValueResult(r.GetValue(0) - r.GetValue(1), true);
    }
}

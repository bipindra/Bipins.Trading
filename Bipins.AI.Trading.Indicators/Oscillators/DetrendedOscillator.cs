using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Momentum;

namespace Bipins.AI.Trading.Indicators.Oscillators;

/// <summary>
/// Detrended Oscillator. Price minus displaced SMA (DPO). Removes trend to show cycles.
/// Reference: William Blau; same as DPO.
/// </summary>
public sealed class DetrendedOscillator : IndicatorBase<SingleValueResult>
{
    private readonly Dpo _dpo;

    /// <summary>
    /// Detrended Oscillator.
    /// </summary>
    /// <param name="period">SMA period (default 20).</param>
    public DetrendedOscillator(int period = 20)
        : base("Detrended Oscillator", "Price minus displaced SMA.", period + period / 2 + 1)
    {
        _dpo = new Dpo(period);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _dpo.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        return _dpo.Update(candle);
    }
}

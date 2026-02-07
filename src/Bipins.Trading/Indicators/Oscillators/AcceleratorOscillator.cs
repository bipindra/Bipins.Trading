using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Oscillators;

/// <summary>
/// Accelerator Oscillator. Awesome Oscillator - SMA(Awesome Oscillator, 5). Measures acceleration of AO.
/// Reference: Bill Williams; Investopedia.
/// </summary>
public sealed class AcceleratorOscillator : IndicatorBase<SingleValueResult>
{
    private readonly AwesomeOscillator _ao;
    private readonly RingBufferDouble _aoBuffer;

    /// <summary>
    /// Accelerator Oscillator.
    /// </summary>
    /// <param name="fastPeriod">AO fast (default 5).</param>
    /// <param name="slowPeriod">AO slow (default 34).</param>
    public AcceleratorOscillator(int fastPeriod = 5, int slowPeriod = 34)
        : base("Accelerator Oscillator", "AO minus SMA(AO,5).", slowPeriod + 5)
    {
        _ao = new AwesomeOscillator(fastPeriod, slowPeriod);
        _aoBuffer = new RingBufferDouble(5);
        AddParameter("Fast", fastPeriod.ToString(), "AO fast");
        AddParameter("Slow", slowPeriod.ToString(), "AO slow");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ao.Reset();
        _aoBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var aoResult = _ao.Update(candle);
        if (!aoResult.IsValid)
            return SingleValueResult.Invalid;
        _aoBuffer.Add(aoResult.Value);
        if (!_aoBuffer.IsFull)
            return new SingleValueResult(aoResult.Value, true);
        double smaAo = _aoBuffer.Sum() / 5;
        return new SingleValueResult(aoResult.Value - smaAo, true);
    }
}

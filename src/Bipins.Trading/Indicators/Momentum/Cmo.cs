using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Chande Momentum Oscillator (CMO). Sum of gains - Sum of losses over period, normalized by sum of gains + losses. Range -100 to 100.
/// Reference: Tushar Chande; "The New Technical Trader".
/// </summary>
public sealed class Cmo : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _changeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Chande Momentum Oscillator.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Cmo(int period = 14)
        : base("CMO", "Chande Momentum Oscillator. -100 to 100.", period + 1)
    {
        Period = period;
        _changeBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _changeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        _changeBuffer.Add(c);
        if (!_changeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumGain = 0, sumLoss = 0;
        for (int i = 0; i < Period; i++)
        {
            double diff = _changeBuffer[i] - _changeBuffer[i + 1];
            if (diff > 0) sumGain += diff;
            else sumLoss -= diff;
        }
        double total = sumGain + sumLoss;
        if (total < 1e-20) return new SingleValueResult(0, true);
        double cmo = 100 * (sumGain - sumLoss) / total;
        return new SingleValueResult(Math.Clamp(cmo, -100, 100), true);
    }
}

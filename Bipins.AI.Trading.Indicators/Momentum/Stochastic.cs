using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Momentum;

/// <summary>
/// Stochastic Oscillator (%K and %D). %K = 100*(Close - Low_n)/(High_n - Low_n), %D = SMA(%K, smoothK).
/// Reference: George Lane; Investopedia.
/// </summary>
public sealed class Stochastic : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private readonly RingBufferDouble _closeBuffer;
    private readonly RingBufferDouble _kBuffer;

    /// <summary>%K period.</summary>
    public int PeriodK { get; }

    /// <summary>%D smoothing period.</summary>
    public int PeriodD { get; }

    /// <summary>
    /// Stochastic Oscillator.
    /// </summary>
    /// <param name="periodK">%K period (default 14).</param>
    /// <param name="periodD">%D smoothing period (default 3).</param>
    public Stochastic(int periodK = 14, int periodD = 3)
        : base("Stochastic", "Stochastic Oscillator %K and %D.", periodK + periodD - 1)
    {
        PeriodK = periodK;
        PeriodD = periodD;
        _highBuffer = new RingBufferDouble(periodK);
        _lowBuffer = new RingBufferDouble(periodK);
        _closeBuffer = new RingBufferDouble(periodK);
        _kBuffer = new RingBufferDouble(periodD);
        AddParameter("PeriodK", periodK.ToString(), "%K period");
        AddParameter("PeriodD", periodD.ToString(), "%D smoothing period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
        _closeBuffer.Clear();
        _kBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        _closeBuffer.Add((double)candle.Close);
        if (!_highBuffer.IsFull)
            return MultiValueResult.Invalid();
        double highN = double.MinValue, lowN = double.MaxValue;
        for (int i = 0; i < PeriodK; i++)
        {
            if (_highBuffer[i] > highN) highN = _highBuffer[i];
            if (_lowBuffer[i] < lowN) lowN = _lowBuffer[i];
        }
        double close = (double)candle.Close;
        double k = (highN - lowN) < 1e-20 ? 50 : 100 * (close - lowN) / (highN - lowN);
        k = Math.Clamp(k, 0, 100);
        _kBuffer.Add(k);
        if (!_kBuffer.IsFull)
            return MultiValueResult.Invalid();
        double d = _kBuffer.Sum() / PeriodD;
        return new MultiValueResult(k, d, true);
    }
}

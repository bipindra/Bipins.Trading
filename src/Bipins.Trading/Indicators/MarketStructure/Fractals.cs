using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MarketStructure;

/// <summary>
/// Fractals. Identifies swing high (high is max of 5 bars) and swing low (low is min of 5 bars). Returns upper fractal and lower fractal values.
/// Reference: Bill Williams; Investopedia.
/// </summary>
public sealed class Fractals : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private const int HalfWindow = 2;

    /// <summary>
    /// Fractals (2 = left/right bars).
    /// </summary>
    /// <param name="leftRightBars">Bars on each side (default 2 for 5-bar fractal).</param>
    public Fractals(int leftRightBars = 2)
        : base("Fractals", "Swing high/low fractal points.", 2 * leftRightBars + 1)
    {
        _highBuffer = new RingBufferDouble(2 * leftRightBars + 1);
        _lowBuffer = new RingBufferDouble(2 * leftRightBars + 1);
        AddParameter("LeftRight", leftRightBars.ToString(), "Bars each side");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        if (!_highBuffer.IsFull)
            return MultiValueResult.Invalid();
        double centerHigh = _highBuffer[HalfWindow];
        double centerLow = _lowBuffer[HalfWindow];
        bool isHigh = true, isLow = true;
        for (int i = 0; i < _highBuffer.Count; i++)
        {
            if (i != HalfWindow && _highBuffer[i] >= centerHigh) isHigh = false;
            if (i != HalfWindow && _lowBuffer[i] <= centerLow) isLow = false;
        }
        double upper = isHigh ? centerHigh : double.NaN;
        double lower = isLow ? centerLow : double.NaN;
        return new MultiValueResult(upper, lower, true);
    }
}

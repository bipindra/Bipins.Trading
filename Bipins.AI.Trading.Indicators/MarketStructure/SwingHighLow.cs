using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MarketStructure;

/// <summary>
/// Swing High/Low Detector. Returns 1 for swing high, -1 for swing low, 0 otherwise over a window.
/// Reference: Standard.
/// </summary>
public sealed class SwingHighLow : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private readonly int _leftRight;

    /// <summary>
    /// Swing High/Low.
    /// </summary>
    /// <param name="leftRightBars">Bars on each side (default 2).</param>
    public SwingHighLow(int leftRightBars = 2)
        : base("Swing High/Low", "Detects swing high and low points.", 2 * leftRightBars + 1)
    {
        _leftRight = leftRightBars;
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
        int center = _leftRight;
        double h = _highBuffer[center], l = _lowBuffer[center];
        bool isHigh = true, isLow = true;
        for (int i = 0; i < _highBuffer.Count; i++)
        {
            if (i != center && _highBuffer[i] >= h) isHigh = false;
            if (i != center && _lowBuffer[i] <= l) isLow = false;
        }
        double swing = isHigh ? 1 : (isLow ? -1 : 0);
        return new MultiValueResult(swing, isHigh ? h : double.NaN, isLow ? l : double.NaN, true);
    }
}

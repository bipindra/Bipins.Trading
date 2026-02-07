using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Arnaud Legoux Moving Average (ALMA). Gaussian-weighted with offset and sigma.
/// Formula: ALMA = Sum(Close_i * exp(-(i - offset)^2 / (2*sigma^2))) / Sum(weights). Default sigma = 6, offset = 0.85*(period-1).
/// Reference: Arnaud Legoux; "Moving Averages".
/// </summary>
public sealed class Alma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _buffer;
    private readonly double[] _weights;
    private readonly int _offset;
    private readonly double _sigma;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Arnaud Legoux Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 9).</param>
    /// <param name="sigma">Gaussian sigma (default 6).</param>
    /// <param name="offset">Offset as fraction of period, 0-1 (default 0.85).</param>
    public Alma(int period = 9, double sigma = 6, double offset = 0.85)
        : base("ALMA", "Arnaud Legoux Moving Average. Gaussian-weighted with configurable offset.", period)
    {
        Period = period;
        _sigma = sigma;
        _offset = (int)Math.Round(offset * (period - 1));
        _buffer = new RingBufferDouble(period);
        _weights = new double[period];
        double sumW = 0;
        for (int i = 0; i < period; i++)
        {
            double w = Math.Exp(-((i - _offset) * (i - _offset)) / (2 * sigma * sigma));
            _weights[i] = w;
            sumW += w;
        }
        for (int i = 0; i < period; i++)
            _weights[i] /= sumW;
        AddParameter("Period", period.ToString(), "Number of periods");
        AddParameter("Sigma", sigma.ToString(), "Gaussian sigma");
        AddParameter("Offset", offset.ToString(), "Offset fraction");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _buffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        _buffer.Add((double)candle.Close);
        if (!_buffer.IsFull)
            return SingleValueResult.Invalid;
        double sum = 0;
        for (int i = 0; i < Period; i++)
            sum += _buffer[i] * _weights[i];
        return new SingleValueResult(sum, true);
    }
}

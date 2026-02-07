using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Volatility;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// SuperTrend. Based on ATR bands; flips when price crosses the band. Upper/Lower band = (High+Low)/2 +/- multiplier*ATR.
/// Reference: Olivier Seban; TradingView.
/// </summary>
public sealed class SuperTrend : IndicatorBase<MultiValueResult>
{
    private readonly Atr _atr;
    private double _upperBand, _lowerBand;
    private double _superTrend;
    private bool _long;
    private bool _initialized;

    /// <summary>ATR period.</summary>
    public int Period { get; }

    /// <summary>ATR multiplier.</summary>
    public double Multiplier { get; }

    /// <summary>
    /// SuperTrend.
    /// </summary>
    /// <param name="period">ATR period (default 10).</param>
    /// <param name="multiplier">ATR multiplier (default 3).</param>
    public SuperTrend(int period = 10, double multiplier = 3)
        : base("SuperTrend", "SuperTrend. ATR-based trend line.", period + 1)
    {
        Period = period;
        Multiplier = multiplier;
        _atr = new Atr(period);
        AddParameter("Period", period.ToString(), "ATR period");
        AddParameter("Multiplier", multiplier.ToString(), "ATR multiplier");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _atr.Reset();
        _initialized = false;
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        var atrResult = _atr.Update(candle);
        if (!atrResult.IsValid)
            return MultiValueResult.Invalid();
        double atr = atrResult.Value;
        double mid = (h + l) / 2;
        double upper = mid + Multiplier * atr;
        double lower = mid - Multiplier * atr;
        if (!_initialized)
        {
            _upperBand = upper;
            _lowerBand = lower;
            _superTrend = lower;
            _long = true;
            _initialized = true;
            return new MultiValueResult(_superTrend, _long ? 1 : 0, true);
        }
        if (_long)
        {
            _lowerBand = Math.Max(lower, _lowerBand);
            if (l <= _lowerBand) { _long = false; _upperBand = upper; _superTrend = _upperBand; }
            else { _superTrend = _lowerBand; _upperBand = Math.Min(upper, _upperBand); }
        }
        else
        {
            _upperBand = Math.Min(upper, _upperBand);
            if (h >= _upperBand) { _long = true; _lowerBand = lower; _superTrend = _lowerBand; }
            else { _superTrend = _upperBand; _lowerBand = Math.Max(lower, _lowerBand); }
        }
        return new MultiValueResult(_superTrend, _long ? 1 : 0, true);
    }
}

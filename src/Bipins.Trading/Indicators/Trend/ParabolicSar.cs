using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Trend;

/// <summary>
/// Parabolic SAR (Stop and Reverse). Trailing stop that flips when price crosses it.
/// Formula: SAR_next = SAR + AF * (EP - SAR). AF starts at step, increases by step when new EP, max at maxAf.
/// Reference: J. Welles Wilder; "New Concepts in Technical Trading Systems".
/// </summary>
public sealed class ParabolicSar : IndicatorBase<SingleValueResult>
{
    private double _sar, _ep, _af;
    private bool _long;
    private bool _initialized;

    /// <summary>Acceleration factor step.</summary>
    public double Step { get; }

    /// <summary>Maximum acceleration factor.</summary>
    public double MaxAf { get; }

    /// <summary>
    /// Parabolic SAR.
    /// </summary>
    /// <param name="step">Acceleration step (default 0.02).</param>
    /// <param name="maxAf">Maximum AF (default 0.2).</param>
    public ParabolicSar(double step = 0.02, double maxAf = 0.2)
        : base("Parabolic SAR", "Parabolic Stop and Reverse. Trailing stop.", 2)
    {
        Step = step;
        MaxAf = maxAf;
        AddParameter("Step", step.ToString(), "Acceleration step");
        AddParameter("MaxAf", maxAf.ToString(), "Maximum acceleration");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _initialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        if (!_initialized)
        {
            _long = true;
            _sar = l;
            _ep = h;
            _af = Step;
            _initialized = true;
            return new SingleValueResult(_sar, true);
        }
        _sar = _sar + _af * (_ep - _sar);
        if (_long)
        {
            if (l <= _sar) { _long = false; _sar = _ep; _ep = l; _af = Step; }
            else
            {
                if (h > _ep) { _ep = h; _af = Math.Min(_af + Step, MaxAf); }
                _sar = Math.Min(_sar, l);
                _sar = Math.Min(_sar, (double)candle.Open);
            }
        }
        else
        {
            if (h >= _sar) { _long = true; _sar = _ep; _ep = h; _af = Step; }
            else
            {
                if (l < _ep) { _ep = l; _af = Math.Min(_af + Step, MaxAf); }
                _sar = Math.Max(_sar, h);
                _sar = Math.Max(_sar, (double)candle.Open);
            }
        }
        return new SingleValueResult(_sar, true);
    }
}

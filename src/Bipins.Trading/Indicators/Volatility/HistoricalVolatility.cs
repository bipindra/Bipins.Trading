using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Historical Volatility. Annualized standard deviation of log returns. HV = StdDev(ln(Close/PrevClose)) * sqrt(periods per year).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class HistoricalVolatility : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _logReturnBuffer;

    /// <summary>Period (number of log returns).</summary>
    public int Period { get; }

    /// <summary>Periods per year (e.g. 252 for daily).</summary>
    public int PeriodsPerYear { get; }

    /// <summary>
    /// Historical Volatility.
    /// </summary>
    /// <param name="period">Lookback period (default 20).</param>
    /// <param name="periodsPerYear">Periods per year for annualization (default 252).</param>
    public HistoricalVolatility(int period = 20, int periodsPerYear = 252)
        : base("Historical Volatility", "Annualized volatility of log returns.", period + 1)
    {
        Period = period;
        PeriodsPerYear = periodsPerYear;
        _logReturnBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Lookback period");
        AddParameter("PeriodsPerYear", periodsPerYear.ToString(), "Periods per year");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _logReturnBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        _logReturnBuffer.Add(c);
        if (!_logReturnBuffer.IsFull)
            return SingleValueResult.Invalid;
        double prev = _logReturnBuffer[0];
        if (prev < 1e-20) prev = 1e-20;
        double logRet = Math.Log(c / prev);
        Span<double> returns = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
        {
            double p = _logReturnBuffer[i + 1];
            if (p < 1e-20) p = 1e-20;
            returns[i] = Math.Log(_logReturnBuffer[i] / p);
        }
        double sd = SpanMath.StdDev(returns, sample: true);
        double hv = sd * Math.Sqrt(PeriodsPerYear);
        return new SingleValueResult(hv, true);
    }
}

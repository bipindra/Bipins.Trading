using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Chaikin Volatility. Rate of change of the trading range (High-Low). ROC of EMA(High-Low) over period.
/// Reference: Marc Chaikin; Investopedia.
/// </summary>
public sealed class ChaikinVolatility : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _hlBuffer;
    private readonly int _rocPeriod;

    /// <summary>EMA period.</summary>
    public int Period { get; }

    /// <summary>
    /// Chaikin Volatility.
    /// </summary>
    /// <param name="period">EMA period (default 10).</param>
    /// <param name="rocPeriod">ROC period (default 10).</param>
    public ChaikinVolatility(int period = 10, int rocPeriod = 10)
        : base("Chaikin Volatility", "ROC of high-low range.", period + rocPeriod)
    {
        Period = period;
        _rocPeriod = rocPeriod;
        _hlBuffer = new RingBufferDouble(rocPeriod + 1);
        AddParameter("Period", period.ToString(), "EMA period");
        AddParameter("RocPeriod", rocPeriod.ToString(), "ROC period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _hlBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double hl = (double)(candle.High - candle.Low);
        _hlBuffer.Add(hl);
        if (!_hlBuffer.IsFull)
            return SingleValueResult.Invalid;
        double prev = _hlBuffer[0];
        if (Math.Abs(prev) < 1e-20) prev = 1e-20;
        double cv = 100 * (hl - prev) / prev;
        return new SingleValueResult(cv, true);
    }
}

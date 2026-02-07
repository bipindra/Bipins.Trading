namespace Bipins.Trading.Indicators.Models;

/// <summary>
/// Represents an OHLCV (Open, High, Low, Close, Volume) price bar (candlestick) for a given time period.
/// Used for stocks, crypto, forex, futures, and options across any timeframe.
/// </summary>
/// <param name="Time">Bar timestamp (start or end of period depending on convention).</param>
/// <param name="Open">Opening price.</param>
/// <param name="High">Highest price during the period.</param>
/// <param name="Low">Lowest price during the period.</param>
/// <param name="Close">Closing price.</param>
/// <param name="Volume">Trading volume (e.g. shares, contracts, base units).</param>
/// <param name="Bid">Optional bid price at bar close.</param>
/// <param name="Ask">Optional ask price at bar close.</param>
/// <param name="VWAP">Optional volume-weighted average price for the bar.</param>
/// <param name="TradesCount">Optional number of trades in the bar.</param>
public readonly record struct Candle(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal? Bid = null,
    decimal? Ask = null,
    decimal? VWAP = null,
    int? TradesCount = null)
{
    /// <summary>Mid price (optional): (High + Low) / 2.</summary>
    public decimal Mid => (High + Low) / 2;

    /// <summary>Typical price: (High + Low + Close) / 3.</summary>
    public decimal TypicalPrice => (High + Low + Close) / 3;

    /// <summary>Weighted close: (High + Low + Close + Close) / 4 = (High + Low + 2*Close) / 4.</summary>
    public decimal WeightedClose => (High + Low + Close + Close) / 4;

    /// <summary>Median price: (High + Low) / 2.</summary>
    public decimal MedianPrice => (High + Low) / 2;

    /// <summary>True range when previous close is available: Max(High-Low, |High-PrevClose|, |Low-PrevClose|).</summary>
    public static decimal TrueRange(decimal high, decimal low, decimal previousClose)
    {
        decimal hl = high - low;
        decimal hc = Math.Abs(high - previousClose);
        decimal lc = Math.Abs(low - previousClose);
        return Math.Max(hl, Math.Max(hc, lc));
    }
}

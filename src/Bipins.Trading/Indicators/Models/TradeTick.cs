namespace Bipins.Trading.Indicators.Models;

/// <summary>
/// Represents a single trade (tick) for tick-based indicators or VWAP/tick analytics.
/// </summary>
/// <param name="Time">Trade timestamp.</param>
/// <param name="Price">Trade price.</param>
/// <param name="Size">Trade size (volume).</param>
/// <param name="IsBuy">True if trade was buyer-initiated (taker buy).</param>
public readonly record struct TradeTick(
    DateTime Time,
    decimal Price,
    decimal Size,
    bool IsBuy);

namespace Bipins.AI.Trading.Indicators.Models;

/// <summary>
/// Represents a single price level in an order book (Level 2 data).
/// </summary>
/// <param name="Price">Price of the level.</param>
/// <param name="Size">Total size (volume) at this level.</param>
/// <param name="IsBid">True for bid side, false for ask side.</param>
public readonly record struct OrderBookLevel(
    decimal Price,
    decimal Size,
    bool IsBid);

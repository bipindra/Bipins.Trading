namespace Bipins.AI.Trading.Indicators.Models;

/// <summary>
/// Represents a bid/ask snapshot at a point in time.
/// Broker-agnostic; used when indicators consume quote data (e.g. spread, mid).
/// </summary>
/// <param name="Time">Quote timestamp.</param>
/// <param name="Bid">Best bid price.</param>
/// <param name="Ask">Best ask price.</param>
/// <param name="BidSize">Optional bid size (volume).</param>
/// <param name="AskSize">Optional ask size (volume).</param>
public readonly record struct Quote(
    DateTime Time,
    decimal Bid,
    decimal Ask,
    decimal? BidSize = null,
    decimal? AskSize = null)
{
    /// <summary>Mid price: (Bid + Ask) / 2.</summary>
    public decimal Mid => (Bid + Ask) / 2;

    /// <summary>Spread: Ask - Bid.</summary>
    public decimal Spread => Ask - Bid;
}

namespace TradingApp.Domain;

public class WatchlistItem
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

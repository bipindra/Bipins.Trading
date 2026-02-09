namespace TradingApp.Domain;

public class Notification
{
    public int Id { get; set; }
    public int AlertId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

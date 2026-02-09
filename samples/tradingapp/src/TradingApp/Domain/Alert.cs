namespace TradingApp.Domain;

public class Alert
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public AlertType AlertType { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }
}

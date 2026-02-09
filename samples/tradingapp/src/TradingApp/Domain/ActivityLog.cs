namespace TradingApp.Domain;

public sealed class ActivityLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty; // Info, Warning, Error, Debug
    public string Category { get; set; } = string.Empty; // AlertWatch, QuoteIngestion, SignalGeneration, etc.
    public string? Symbol { get; set; }
    public string? AlertId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; } // JSON for additional data
}

namespace TradingApp.Domain;

public class AlpacaSettings
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}

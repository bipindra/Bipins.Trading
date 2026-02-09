using Bipins.Trading.Domain;

namespace TradingApp.Application;

public interface ISignalService
{
    Task<IReadOnlyList<SignalDto>> GetSignalsAsync(string symbol, string? strategyName = null, CancellationToken ct = default);
}

public sealed class SignalDto
{
    public string Strategy { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Reason { get; set; }
}

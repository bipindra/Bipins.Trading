using TradingApp.Domain;

namespace TradingApp.Application;

public interface IAlpacaSettingsRepository
{
    Task<AlpacaSettings?> GetAsync(CancellationToken ct = default);
    Task SaveAsync(AlpacaSettings settings, CancellationToken ct = default);
}

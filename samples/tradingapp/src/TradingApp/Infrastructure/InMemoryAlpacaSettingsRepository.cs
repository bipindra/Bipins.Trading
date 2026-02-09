using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

/// <summary>
/// In-memory Alpaca settings store for testing. Settings are lost when the app restarts.
/// </summary>
public sealed class InMemoryAlpacaSettingsRepository : IAlpacaSettingsRepository
{
    private static readonly object Lock = new();
    private static AlpacaSettings? _settings;

    public Task<AlpacaSettings?> GetAsync(CancellationToken ct = default)
    {
        lock (Lock)
        {
            return Task.FromResult(_settings);
        }
    }

    public Task SaveAsync(AlpacaSettings settings, CancellationToken ct = default)
    {
        lock (Lock)
        {
            _settings = new AlpacaSettings
            {
                Id = settings.Id,
                ApiKey = settings.ApiKey ?? string.Empty,
                ApiSecret = settings.ApiSecret ?? string.Empty,
                BaseUrl = settings.BaseUrl ?? string.Empty
            };
            return Task.CompletedTask;
        }
    }
}

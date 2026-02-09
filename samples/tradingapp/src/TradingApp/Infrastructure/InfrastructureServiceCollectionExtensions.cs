using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingApp.Application;

namespace TradingApp.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        // Data Protection is configured in Program.cs (persistent key path)
        services.AddScoped<SecretEncryption>();
        services.AddScoped<IWatchlistRepository, WatchlistRepository>();
        // In-memory for testing; switch to AlpacaSettingsRepository for DB persistence
        services.AddSingleton<IAlpacaSettingsRepository, InMemoryAlpacaSettingsRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        return services;
    }
}

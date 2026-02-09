using Microsoft.Extensions.DependencyInjection;

namespace TradingApp.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAlpacaService, AlpacaService>();
        services.AddScoped<ISignalService, SignalService>();
        services.AddScoped<WatchlistService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<AlertService>(sp => new AlertService(
            sp.GetRequiredService<IAlertRepository>(),
            sp.GetService<IActivityLogRepository>()));
        services.AddScoped<NotificationService>();
        return services;
    }
}

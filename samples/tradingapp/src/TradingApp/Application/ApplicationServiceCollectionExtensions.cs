using Microsoft.Extensions.DependencyInjection;

namespace TradingApp.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAlpacaService, AlpacaService>();
        services.AddScoped<WatchlistService>();
        services.AddScoped<SettingsService>();
        services.AddScoped<AlertService>();
        services.AddScoped<NotificationService>();
        return services;
    }
}

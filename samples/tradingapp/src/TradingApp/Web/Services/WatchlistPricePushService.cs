using Microsoft.AspNetCore.SignalR;
using TradingApp.Application;
using TradingApp.Web.Hubs;

namespace TradingApp.Web.Services;

public sealed class WatchlistPricePushService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);
    private readonly IServiceProvider _services;

    public WatchlistPricePushService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First push after short delay so clients can connect and subscribe
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PushPricesAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task PushPricesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IWatchlistPriceSubscriptionStore>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<WatchlistPriceHub>>();
        var alpaca = scope.ServiceProvider.GetRequiredService<IAlpacaService>();

        var symbols = store.GetSubscribedSymbols();
        if (symbols.Count == 0) return;

        foreach (var symbol in symbols)
        {
            try
            {
                var price = await alpaca.GetLatestPriceAsync(symbol, ct);
                if (price.HasValue)
                    await hubContext.Clients.All.SendAsync("PriceUpdate", symbol, (double)price.Value, ct);
            }
            catch { /* ignore per-symbol errors */ }
        }
    }
}

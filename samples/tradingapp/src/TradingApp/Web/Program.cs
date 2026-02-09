using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Polly;
using Polly.Retry;
using TradingApp.Application;
using TradingApp.Infrastructure;
using TradingApp.Web.Hubs;
using TradingApp.Web.Infrastructure;
using TradingApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(o => o.Filters.Add<ValidationErrorFilter>());
builder.Services.Configure<RouteOptions>(o =>
{
    o.ConstraintMap.Add("notsearch", typeof(NotSearchRouteConstraint));
    o.ConstraintMap.Add("notapi", typeof(NotApiPathConstraint));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Data Protection with persistent key path (required for Settings save; avoids 500 on encrypt/decrypt)
var keyPath = Path.Combine(builder.Environment.ContentRootPath, "dp-keys");
Directory.CreateDirectory(keyPath);
builder.Services.AddDataProtection()
    .SetApplicationName("TradingApp")
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath));

var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tradingapp.db";
builder.Services.AddInfrastructure(conn);
builder.Services.AddApplication();
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddSingleton<IWatchlistPriceSubscriptionStore, WatchlistPriceSubscriptionStore>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<AlertWatchHostedService>();
builder.Services.AddHostedService<WatchlistPricePushService>();

// Optional: execute live order when an alert triggers (Bipins.Trading + Alpaca)
var executeOnTrigger = builder.Configuration.GetValue<bool>("Trading:ExecuteOnTrigger");
if (executeOnTrigger)
{
    builder.Services.AddSingleton<IFillReceiver, TradingAppFillReceiver>();
    builder.Services.AddSingleton<IExecutionAdapter>(sp =>
        new LiveAlpacaExecutionAdapter(
            sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetRequiredService<IAlpacaSettingsRepository>(),
            sp.GetRequiredService<IFillReceiver>(),
            sp.GetRequiredService<ILogger<LiveAlpacaExecutionAdapter>>()));
    builder.Services.AddScoped<IExecutionEngine, BipinsExecutionEngine>();
}

// Named HttpClient for Alpaca with retry (2 retries, 200ms delay)
builder.Services.AddHttpClient("Alpaca")
    .AddPolicyHandler(Policy.Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(200)));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
    app.UseSwaggerUI();

app.UseSwagger();

// Serve SPA static files before routing so / and /assets/* are served from dist, not the fallback
var clientDist = Path.Combine(app.Environment.ContentRootPath, "Web", "ClientApp", "dist");
if (Directory.Exists(clientDist))
{
    var fileProvider = new PhysicalFileProvider(clientDist);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider, RequestPath = "" });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider, RequestPath = "" });
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHub<WatchlistPriceHub>("/hubs/watchlist-price");

if (Directory.Exists(clientDist))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions { FileProvider = new PhysicalFileProvider(clientDist) });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

app.Run();

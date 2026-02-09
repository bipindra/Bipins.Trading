using System.Data.Common;
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

// Configure logging to exclude Microsoft.* logs from console
builder.Logging.AddFilter("Microsoft.*", LogLevel.None);
builder.Logging.AddFilter("System.*", LogLevel.Warning); // Only show warnings and above for System.*

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

// Register execution adapter for manual trading (always available)
builder.Services.AddSingleton<IFillReceiver, TradingAppFillReceiver>();
builder.Services.AddSingleton<IExecutionAdapter>(sp =>
    new LiveAlpacaExecutionAdapter(
        sp.GetRequiredService<IHttpClientFactory>(),
        sp.GetRequiredService<IAlpacaSettingsRepository>(),
        sp.GetRequiredService<IFillReceiver>(),
        sp.GetRequiredService<ILogger<LiveAlpacaExecutionAdapter>>()));

// Register execution engine for auto-execute when alerts trigger
// This allows alerts with EnableAutoExecute=true to automatically execute trades
builder.Services.AddScoped<IExecutionEngine, BipinsExecutionEngine>();

// Named HttpClient for Alpaca with retry (2 retries, 200ms delay)
builder.Services.AddHttpClient("Alpaca")
    .AddPolicyHandler(Policy.Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromMilliseconds(200)));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
        
        // One-time fix: Add missing columns if migrations were marked as applied but columns don't exist
        // This handles the case where migration history says it's applied but columns are missing
        try
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "PRAGMA table_info(Alerts)";
            using var reader = await checkCommand.ExecuteReaderAsync();
            var columns = new List<string>();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(1));
            }
            await reader.CloseAsync();
            
            if (!columns.Contains("ComparisonType"))
            {
                logger.LogWarning("ComparisonType column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN ComparisonType INTEGER");
            }
            if (!columns.Contains("Threshold"))
            {
                logger.LogWarning("Threshold column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN Threshold TEXT");
            }
            if (!columns.Contains("Timeframe"))
            {
                logger.LogWarning("Timeframe column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN Timeframe TEXT");
            }
            if (!columns.Contains("EnableAutoExecute"))
            {
                logger.LogWarning("EnableAutoExecute column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN EnableAutoExecute INTEGER NOT NULL DEFAULT 0");
            }
            if (!columns.Contains("OrderQuantity"))
            {
                logger.LogWarning("OrderQuantity column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderQuantity TEXT");
            }
            if (!columns.Contains("OrderType"))
            {
                logger.LogWarning("OrderType column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderType INTEGER");
            }
            if (!columns.Contains("OrderSideOverride"))
            {
                logger.LogWarning("OrderSideOverride column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderSideOverride INTEGER");
            }
            if (!columns.Contains("OrderLimitPrice"))
            {
                logger.LogWarning("OrderLimitPrice column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderLimitPrice TEXT");
            }
            if (!columns.Contains("OrderStopPrice"))
            {
                logger.LogWarning("OrderStopPrice column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderStopPrice TEXT");
            }
            if (!columns.Contains("OrderTimeInForce"))
            {
                logger.LogWarning("OrderTimeInForce column missing despite migrations. Adding it manually...");
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE Alerts ADD COLUMN OrderTimeInForce INTEGER");
            }
            
            await connection.CloseAsync();
        }
        catch (Exception fixEx)
        {
            logger.LogWarning(fixEx, "Failed to check/add missing columns, but continuing anyway");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations: {Error}", ex.Message);
        throw; // Re-throw to prevent app from starting with invalid database state
    }
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
app.MapHub<ActivityLogHub>("/hubs/activity-log");

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

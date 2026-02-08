using Alpaca.Markets;
using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Bipins.Trading.Execution.Alpaca;
using Bipins.Trading.Risk;
using Bipins.Trading.Strategies;
using Microsoft.Extensions.Logging;

namespace Bipins.Trading.Samples.Alpaca;

/// <summary>
/// Live trading example using Alpaca broker integration.
/// 
/// This example demonstrates:
/// - Connecting to Alpaca API (paper or live)
/// - Real-time market data streaming
/// - Order execution through Alpaca
/// - Portfolio tracking
/// - Risk management
/// 
/// Setup:
/// 1. Get your Alpaca API credentials from https://alpaca.markets/
/// 2. Set environment variables: ALPACA_API_KEY_ID and ALPACA_SECRET_KEY
/// 3. Or modify the credentials directly in this file (not recommended for production)
/// </summary>
class Program
{
    private static async Task Main(string[] args)
    {
        // Get credentials from environment or use defaults (for demo only)
        var apiKeyId = Environment.GetEnvironmentVariable("ALPACA_API_KEY_ID") 
            ?? throw new InvalidOperationException("ALPACA_API_KEY_ID environment variable not set");
        var secretKey = Environment.GetEnvironmentVariable("ALPACA_SECRET_KEY") 
            ?? throw new InvalidOperationException("ALPACA_SECRET_KEY environment variable not set");

        // Use paper trading by default (set to Environments.Live for production)
        var environment = args.Length > 0 && args[0] == "live" 
            ? Environments.Live 
            : Environments.Paper;

        Console.WriteLine($"Connecting to Alpaca {environment} environment...");

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<Program>();

        // Initialize Alpaca clients
        var key = new SecretKey(apiKeyId, secretKey);
        var tradingClient = environment.GetAlpacaTradingClient(key);
        var dataClient = environment.GetAlpacaDataClient(key);
        var streamingClient = environment.GetAlpacaStreamingClient(key);

        try
        {
            // Verify connection
            Console.WriteLine("Attempting to connect to Alpaca...");
            var account = await tradingClient.GetAccountAsync();
            var equity = account.Equity ?? 0m;
            Console.WriteLine($"Connected! Account Status: {account.Status}, Equity: ${equity:F2}");

            // Create portfolio service
            var portfolioService = new PortfolioService();
            portfolioService.Reset((decimal)equity);

            // Create event bus
            var eventBus = new InMemoryEventBus();

            // Create market data feed
            var marketDataFeed = new AlpacaMarketDataFeed(dataClient, streamingClient, 
                loggerFactory.CreateLogger<AlpacaMarketDataFeed>());

            // Create execution adapter
            var fillReceiver = new LiveFillReceiver(portfolioService, eventBus,
                loggerFactory.CreateLogger<LiveFillReceiver>());
            var executionAdapter = new AlpacaExecutionAdapter(tradingClient, fillReceiver,
                loggerFactory.CreateLogger<AlpacaExecutionAdapter>());

            // Setup risk management
            var riskManager = new CompositeRiskManager(new List<IRiskPolicy>
            {
                new MaxPositionsPolicy(5), // Max 5 positions
                new MaxDailyLossPolicy(0.05m) // Max 5% daily loss
            });

            var positionSizer = new PercentEquitySizer(0.10m); // 10% of equity per position

            // Subscribe to events
            eventBus.Subscribe<FillEvent>(evt =>
            {
                Console.WriteLine($"[FILL] {evt.Fill.Symbol} {evt.Fill.Side} {evt.Fill.Quantity} @ ${evt.Fill.Price:F2}");
            });

            eventBus.Subscribe<SignalEventEvent>(evt =>
            {
                Console.WriteLine($"[SIGNAL] {evt.Signal.Symbol} {evt.Signal.SignalType} at {evt.Signal.Time}");
            });

            // Create strategies
            var strategies = new List<IStrategy>
            {
                new EmaCrossoverStrategy(9, 21),
                new RsiMeanReversionStrategy(14, 30, 70)
            };

            // Create live trading engine
            var liveEngine = new LiveTradingEngine(
                marketDataFeed,
                eventBus,
                portfolioService,
                riskManager,
                positionSizer,
                executionAdapter,
                strategies,
                loggerFactory.CreateLogger<LiveTradingEngine>());

            // Start live trading
            Console.WriteLine("Starting live trading engine...");
            Console.WriteLine("Press Ctrl+C to stop.");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            await liveEngine.StartAsync(cts.Token);

            Console.WriteLine("Live trading stopped.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during live trading");
            throw;
        }
        finally
        {
            tradingClient?.Dispose();
            dataClient?.Dispose();
            streamingClient?.Dispose();
        }
    }
}

using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Bipins.Trading.Execution.Alpaca;
using Bipins.Trading.Risk;
using Microsoft.Extensions.Logging;

namespace Bipins.Trading.Samples.Alpaca;

/// <summary>
/// Live trading engine that runs strategies in real-time using market data feeds.
/// </summary>
public sealed class LiveTradingEngine : IDisposable
{
    private readonly AlpacaMarketDataFeed _marketDataFeed;
    private readonly IEventBus _eventBus;
    private readonly IPortfolioService _portfolio;
    private readonly IRiskManager _riskManager;
    private readonly IPositionSizer _positionSizer;
    private readonly IExecutionAdapter _execution;
    private readonly IReadOnlyList<IStrategy> _strategies;
    private readonly ILogger<LiveTradingEngine>? _logger;
    private readonly IndicatorProvider _indicatorProvider = new();
    private readonly Dictionary<string, List<Candle>> _candleBuffers = new();
    private readonly CancellationTokenSource _cts = new();

    public LiveTradingEngine(
        AlpacaMarketDataFeed marketDataFeed,
        IEventBus eventBus,
        IPortfolioService portfolio,
        IRiskManager riskManager,
        IPositionSizer positionSizer,
        IExecutionAdapter execution,
        IReadOnlyList<IStrategy> strategies,
        ILogger<LiveTradingEngine>? logger = null)
    {
        _marketDataFeed = marketDataFeed ?? throw new ArgumentNullException(nameof(marketDataFeed));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
        _riskManager = riskManager ?? throw new ArgumentNullException(nameof(riskManager));
        _positionSizer = positionSizer ?? throw new ArgumentNullException(nameof(positionSizer));
        _execution = execution ?? throw new ArgumentNullException(nameof(execution));
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting live trading engine with {Count} strategies", _strategies.Count);

        // Subscribe to symbols from all strategies
        var symbols = _strategies
            .SelectMany(s => GetStrategySymbols(s))
            .Distinct()
            .ToList();

        foreach (var symbol in symbols)
        {
            await _marketDataFeed.SubscribeToBarsAsync(
                symbol,
                "1MIN", // Default timeframe - could be configurable
                candle => OnNewBar(symbol, candle),
                cancellationToken);

            _candleBuffers[symbol] = new List<Candle>();
        }

        _logger?.LogInformation("Subscribed to {Count} symbols: {Symbols}", 
            symbols.Count, string.Join(", ", symbols));

        // Keep running until cancelled
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("Live trading engine stopped");
        }
    }

    private IEnumerable<string> GetStrategySymbols(IStrategy strategy)
    {
        // In a real implementation, you'd configure symbols per strategy
        // For now, return a default set
        yield return "SPY"; // S&P 500 ETF
        yield return "QQQ"; // Nasdaq ETF
    }

    private void OnNewBar(string symbol, Candle candle)
    {
        try
        {
            // Add to buffer
            if (!_candleBuffers.TryGetValue(symbol, out var buffer))
            {
                buffer = new List<Candle>();
                _candleBuffers[symbol] = buffer;
            }

            buffer.Add(candle);

            // Keep only recent candles (e.g., last 1000)
            if (buffer.Count > 1000)
                buffer.RemoveAt(0);

            // Process each strategy
            foreach (var strategy in _strategies)
            {
                ProcessStrategy(strategy, symbol, buffer);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing bar for {Symbol}", symbol);
        }
    }

    private void ProcessStrategy(IStrategy strategy, string symbol, List<Candle> candles)
    {
        if (candles.Count < strategy.Warmup.RequiredCandleCount)
            return; // Not enough data yet

        // Create strategy context
        var portfolioState = _portfolio.GetState();
        var currentPosition = portfolioState.Positions.TryGetValue(symbol, out var pos) 
            ? pos 
            : new Position(symbol, PositionSide.Flat, 0, 0, 0, 0);

        var context = new StrategyContext
        {
            Symbol = symbol,
            Timeframe = "1MIN",
            Candles = candles,
            Portfolio = portfolioState,
            CurrentPosition = currentPosition,
            Indicators = _indicatorProvider,
            Parameters = new Dictionary<string, object>(),
            CancellationToken = _cts.Token
        };

        // Run strategy
        var result = strategy.OnBar(context);

        // Process signals
        foreach (var signal in result.Signals)
        {
            _eventBus.Publish(new SignalEventEvent(signal.Time, signal));

            if (signal.SignalType == SignalType.EntryLong || signal.SignalType == SignalType.EntryShort)
            {
                HandleEntrySignal(signal, context);
            }
            else if (signal.SignalType == SignalType.ExitLong || signal.SignalType == SignalType.ExitShort)
            {
                HandleExitSignal(signal, context);
            }
        }
    }

    private void HandleEntrySignal(SignalEvent signal, StrategyContext context)
    {
        try
        {
            // Create initial order intent
            var orderIntent = new OrderIntent(
                Strategy: signal.Strategy,
                Symbol: signal.Symbol,
                Time: signal.Time,
                Side: signal.SignalType == SignalType.EntryLong ? OrderSide.Buy : OrderSide.Sell,
                OrderType: OrderType.Market,
                TimeInForce: TimeInForce.Day,
                Quantity: null,
                LimitPrice: signal.Price,
                StopPrice: null,
                RiskEnvelope: null,
                Reason: signal.Reason,
                Metrics: signal.Metrics,
                ClientOrderId: $"{signal.Strategy}_{signal.Symbol}_{signal.Time:yyyyMMddHHmmss}");

            // Check risk
            var riskDecision = _riskManager.Approve(orderIntent, context.Portfolio);

            if (!riskDecision.Approved)
            {
                _logger?.LogWarning("Risk manager rejected signal: {Reason}", riskDecision.Reason);
                return;
            }

            // Size position
            var sizedIntent = _positionSizer.Size(orderIntent, context.Portfolio, signal.Price ?? 0m);

            if (!sizedIntent.Quantity.HasValue || sizedIntent.Quantity.Value <= 0)
                return;

            // Update with risk envelope from the original intent if available
            // Note: RiskDecisionEvent doesn't contain Envelope, it's in the Intent
            orderIntent = sizedIntent with { RiskEnvelope = sizedIntent.RiskEnvelope };

            // Submit order
            _execution.Submit(orderIntent);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling entry signal: {Signal}", signal);
        }
    }

    private void HandleExitSignal(SignalEvent signal, StrategyContext context)
    {
        try
        {
            if (context.CurrentPosition.Side == PositionSide.Flat)
                return; // No position to exit

            // Exit entire position
            var quantity = context.CurrentPosition.Quantity;

            var orderIntent = new OrderIntent(
                Strategy: signal.Strategy,
                Symbol: signal.Symbol,
                Time: signal.Time,
                Side: context.CurrentPosition.Side == PositionSide.Long ? OrderSide.Sell : OrderSide.Buy,
                OrderType: OrderType.Market,
                TimeInForce: TimeInForce.Day,
                Quantity: quantity,
                LimitPrice: null,
                StopPrice: null,
                RiskEnvelope: null,
                Reason: signal.Reason,
                Metrics: signal.Metrics,
                ClientOrderId: $"{signal.Strategy}_{signal.Symbol}_EXIT_{signal.Time:yyyyMMddHHmmss}");

            _execution.Submit(orderIntent);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error handling exit signal: {Signal}", signal);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Charting;
using Bipins.Trading.Execution;
using Bipins.Trading.Risk;

namespace Bipins.Trading.Engine;

public sealed class BacktestRunner
{
    private readonly IMarketDataFeed _feed;
    private readonly IEventBus _bus;
    private readonly IPortfolioService _portfolio;
    private readonly IRiskManager _risk;
    private readonly IPositionSizer _sizer;
    private readonly IExecutionAdapter _execution;
    private readonly IChartSink? _chartSink;
    private readonly IReadOnlyList<IStrategy> _strategies;

    public BacktestRunner(
        IMarketDataFeed feed,
        IEventBus bus,
        IPortfolioService portfolio,
        IRiskManager risk,
        IPositionSizer sizer,
        IExecutionAdapter execution,
        IReadOnlyList<IStrategy> strategies,
        IChartSink? chartSink = null)
    {
        _feed = feed;
        _bus = bus;
        _portfolio = portfolio;
        _risk = risk;
        _sizer = sizer;
        _execution = execution;
        _strategies = strategies;
        _chartSink = chartSink;
    }

    public BacktestResult Run(BacktestConfig config, BacktestFillReceiver? fillReceiver = null)
    {
        _portfolio.Reset(config.InitialCash);
        var indicatorProvider = new IndicatorProvider();
        var equityCurve = new List<(DateTime Time, decimal Equity)>();

        foreach (var symbol in config.Symbols)
        {
            foreach (var timeframe in config.Timeframes)
            {
                var candles = _feed.GetCandles(symbol, timeframe, config.Start, config.End).OrderBy(c => c.Time).ToList();
                if (candles.Count == 0) continue;

                var warmup = config.WarmupBars;
                for (var i = warmup; i < candles.Count; i++)
                {
                    var slice = candles.Take(i + 1).ToList();
                    var current = candles[i];
                    indicatorProvider.ClearCache();

                    var portfolioState = _portfolio.GetState();
                    var position = portfolioState.Positions.TryGetValue(symbol, out var p) ? p : new Position(symbol, PositionSide.Flat, 0, 0, 0, 0);

                    foreach (var strategy in _strategies)
                    {
                        if (slice.Count < strategy.Warmup.RequiredCandleCount) continue;

                        var ctx = new StrategyContext
                        {
                            Symbol = symbol,
                            Timeframe = timeframe,
                            Candles = slice,
                            Portfolio = portfolioState,
                            CurrentPosition = position,
                            Indicators = indicatorProvider,
                            Parameters = (config.StrategyParameters ?? new Dictionary<string, IReadOnlyDictionary<string, object>>()).GetValueOrDefault(strategy.Name, new Dictionary<string, object>())
                        };

                        var result = strategy.OnBar(ctx);

                        foreach (var signal in result.Signals)
                        {
                            _bus.Publish(new SignalEventEvent(signal.Time, signal));
                            _chartSink?.Publish(signal);
                        }

                        foreach (var order in result.Orders)
                        {
                            var sized = _sizer.Size(order, portfolioState, current.Close, null);
                            var decision = _risk.Approve(sized, portfolioState, null);
                            _bus.Publish(decision);
                            if (decision.Approved && sized.Quantity.HasValue && sized.Quantity.Value > 0)
                            {
                                var fillPrice = sized.LimitPrice ?? sized.StopPrice ?? current.Close;
                                var orderToSubmit = sized with { LimitPrice = fillPrice };
                                _execution.Submit(orderToSubmit);
                                _bus.Publish(new OrderIntentEvent(sized.Time, sized));
                            }
                        }
                    }

                    portfolioState = _portfolio.GetState();
                    equityCurve.Add((current.Time, portfolioState.Equity));
                }
            }
        }

        _chartSink?.Flush();
        var finalState = _portfolio.GetState();
        var trades = fillReceiver?.Trades ?? Array.Empty<Fill>();
        return new BacktestResult(finalState.Equity, equityCurve, trades);
    }
}

public sealed class BacktestConfig
{
    public IReadOnlyList<string> Symbols { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Timeframes { get; init; } = Array.Empty<string>();
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
    public decimal InitialCash { get; init; } = 100000m;
    public int WarmupBars { get; init; } = 100;
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>>? StrategyParameters { get; init; }
}

public sealed class BacktestResult
{
    public decimal FinalEquity { get; }
    public IReadOnlyList<(DateTime Time, decimal Equity)> EquityCurve { get; }
    public IReadOnlyList<Fill> Trades { get; }

    public BacktestResult(decimal finalEquity, IReadOnlyList<(DateTime Time, decimal Equity)> equityCurve, IReadOnlyList<Fill> trades)
    {
        FinalEquity = finalEquity;
        EquityCurve = equityCurve;
        Trades = trades;
    }
}

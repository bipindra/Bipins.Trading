using Bipins.Trading.Domain;
using Bipins.Trading.Engine;
using Bipins.Trading.Strategies;
using Microsoft.Extensions.Logging;

namespace TradingApp.Application;

public sealed class SignalService : ISignalService
{
    private readonly IAlpacaService _alpacaService;
    private readonly ILogger<SignalService> _logger;
    private readonly Dictionary<string, IStrategy> _strategies;

    public SignalService(IAlpacaService alpacaService, ILogger<SignalService> logger)
    {
        _alpacaService = alpacaService;
        _logger = logger;
        
        // Initialize available strategies
        _strategies = new Dictionary<string, IStrategy>
        {
            { "EmaCrossover", new EmaCrossoverStrategy(12, 26) },
            { "RsiMeanReversion", new RsiMeanReversionStrategy(14, 30, 70) },
            { "BreakoutDonchian", new BreakoutDonchianStrategy(20) }
        };
    }

    public async Task<IReadOnlyList<SignalDto>> GetSignalsAsync(string symbol, string? strategyName = null, CancellationToken ct = default)
    {
        var allSignals = new List<SignalDto>();
        var strategiesToRun = string.IsNullOrWhiteSpace(strategyName)
            ? _strategies.Values.ToList()
            : _strategies.TryGetValue(strategyName, out var strategy) 
                ? new List<IStrategy> { strategy }
                : new List<IStrategy>();

        if (strategiesToRun.Count == 0)
        {
            _logger.LogWarning("No strategies found for {StrategyName}", strategyName);
            return Array.Empty<SignalDto>();
        }

        try
        {
            // Get historical bars (1Day timeframe, last 100 days for warmup)
            var end = DateTime.UtcNow;
            var start = end.AddDays(-100);
            var bars = await _alpacaService.GetBarsAsync(symbol, "1Day", start, end, ct);

            if (bars.Count == 0)
            {
                _logger.LogWarning("No bars found for {Symbol}", symbol);
                return Array.Empty<SignalDto>();
            }

            var indicatorProvider = new IndicatorProvider();
            var portfolioState = new PortfolioState(10000m, 10000m, new Dictionary<string, Position>());
            var currentPosition = new Position(symbol, PositionSide.Flat, 0, 0, 0, 0);

            foreach (var strategyToRun in strategiesToRun)
            {
                try
                {
                    // Check if we have enough bars for warmup
                    var warmupBars = strategyToRun.Warmup.RequiredCandleCount;
                    if (bars.Count < warmupBars)
                    {
                        _logger.LogDebug("Not enough bars for {Strategy} on {Symbol}: {Count} < {Warmup}",
                            strategyToRun.Name, symbol, bars.Count, warmupBars);
                        continue;
                    }

                    var context = new StrategyContext
                    {
                        Symbol = symbol,
                        Timeframe = "1Day",
                        Candles = bars,
                        Portfolio = portfolioState,
                        CurrentPosition = currentPosition,
                        Indicators = indicatorProvider,
                        Parameters = new Dictionary<string, object>(),
                        CancellationToken = ct
                    };

                    var result = strategyToRun.OnBar(context);

                    foreach (var signal in result.Signals)
                    {
                        allSignals.Add(new SignalDto
                        {
                            Strategy = signal.Strategy,
                            Symbol = signal.Symbol,
                            Time = signal.Time,
                            SignalType = signal.SignalType.ToString(),
                            Price = signal.Price,
                            Reason = signal.Reason
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running strategy {Strategy} for {Symbol}", strategyToRun.Name, symbol);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signals for {Symbol}", symbol);
        }

        return allSignals.OrderByDescending(s => s.Time).ToList();
    }
}

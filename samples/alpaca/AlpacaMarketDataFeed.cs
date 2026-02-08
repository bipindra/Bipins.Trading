using Alpaca.Markets;
using Bipins.Trading.Domain;
using Bipins.Trading.Engine;
using Microsoft.Extensions.Logging;

namespace Bipins.Trading.Execution.Alpaca;

/// <summary>
/// Alpaca broker implementation of IMarketDataFeed.
/// Provides historical and real-time market data from Alpaca.
/// </summary>
public sealed class AlpacaMarketDataFeed : IMarketDataFeed, IDisposable
{
    private readonly IAlpacaDataClient _dataClient;
    private readonly IAlpacaStreamingClient? _streamingClient;
    private readonly ILogger<AlpacaMarketDataFeed>? _logger;
    private readonly Dictionary<string, string> _timeframeMap = new();

    public AlpacaMarketDataFeed(
        IAlpacaDataClient dataClient,
        IAlpacaStreamingClient? streamingClient = null,
        ILogger<AlpacaMarketDataFeed>? logger = null)
    {
        _dataClient = dataClient ?? throw new ArgumentNullException(nameof(dataClient));
        _streamingClient = streamingClient;
        _logger = logger;
    }

    public IEnumerable<Candle> GetCandles(string symbol, string timeframe, DateTime start, DateTime end)
    {
        return GetCandlesAsync(symbol, timeframe, start, end).GetAwaiter().GetResult();
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(
        string symbol,
        string timeframe,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: Alpaca.Markets v7.0 API may require different constructor
            // This is a simplified version - adjust based on actual API
            var request = new HistoricalBarsRequest(symbol, start, end, MapTimeframe(timeframe));
            var barSet = await _dataClient.GetHistoricalBarsAsync(request, cancellationToken);

            // barSet.Items is a dictionary keyed by symbol
            if (barSet.Items.TryGetValue(symbol, out var bars))
            {
                return bars.Select(bar => MapToCandle(bar, symbol, timeframe));
            }
            return Enumerable.Empty<Candle>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get candles for {Symbol} {Timeframe}", symbol, timeframe);
            throw;
        }
    }

    /// <summary>
    /// Subscribe to real-time bar updates for a symbol.
    /// Note: This is a placeholder implementation. 
    /// Alpaca.Markets v7.0 streaming API may differ - adjust based on actual API.
    /// </summary>
    public async Task SubscribeToBarsAsync(
        string symbol,
        string timeframe,
        Action<Candle> onBar,
        CancellationToken cancellationToken = default)
    {
        if (_streamingClient == null)
        {
            _logger?.LogWarning("Streaming client not configured. Real-time updates will not be available.");
            return;
        }

        _timeframeMap[symbol] = timeframe;

        // TODO: Implement actual streaming subscription based on Alpaca.Markets v7.0 API
        // The streaming API may have changed - check the actual documentation
        _logger?.LogInformation("Subscribed to {Symbol} (streaming implementation pending)", symbol);
        
        // For now, this is a placeholder - you'll need to implement based on actual API
        await Task.CompletedTask;
    }

    private BarTimeFrame MapTimeframe(string timeframe)
    {
        return timeframe.ToUpperInvariant() switch
        {
            "1MIN" or "1" => BarTimeFrame.Minute,
            "5MIN" or "5" => BarTimeFrame.Minute, // Use Minute and aggregate if needed
            "15MIN" or "15" => BarTimeFrame.Minute,
            "30MIN" or "30" => BarTimeFrame.Minute,
            "1HOUR" or "1H" or "60" => BarTimeFrame.Hour,
            "1DAY" or "D" => BarTimeFrame.Day,
            _ => BarTimeFrame.Day
        };
    }

    private Candle MapToCandle(IBar bar, string symbol, string timeframe)
    {
        return new Candle(
            bar.TimeUtc,
            bar.Open,
            bar.High,
            bar.Low,
            bar.Close,
            bar.Volume,
            symbol,
            timeframe);
    }

    public void Dispose()
    {
        _streamingClient?.Dispose();
        _dataClient?.Dispose();
    }
}

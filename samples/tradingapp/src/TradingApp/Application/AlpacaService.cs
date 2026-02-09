using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bipins.Trading.Domain;
using Microsoft.Extensions.Logging;
using TradingApp.Application.DTOs;
using TradingApp.Domain;

namespace TradingApp.Application;

public sealed class AlpacaService : IAlpacaService
{
    private const int SearchResultLimit = 20;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAlpacaSettingsRepository _settingsRepository;
    private readonly ILogger<AlpacaService> _logger;

    public AlpacaService(
        IHttpClientFactory httpClientFactory,
        IAlpacaSettingsRepository settingsRepository,
        ILogger<AlpacaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    private async Task<AlpacaSettings> GetSettingsOrThrowAsync(CancellationToken ct)
    {
        var settings = await _settingsRepository.GetAsync(ct);
        if (settings == null || string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ApiSecret))
            throw new InvalidOperationException("Stock data provider is not configured.");
        return settings;
    }

    public async Task<IReadOnlyList<StockSearchResult>> SearchAssetsAsync(string query, CancellationToken ct = default)
    {
        var settings = await GetSettingsOrThrowAsync(ct);

        var baseUrl = (settings.BaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://paper-api.alpaca.markets";

        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(10);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v2/assets?status=active&asset_class=us_equity");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);

        try
        {
            var response = await client.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<List<AlpacaAssetDto>>(JsonOptions, ct) ?? new List<AlpacaAssetDto>();
            var q = (query ?? "").Trim().ToUpperInvariant();
            var filtered = string.IsNullOrEmpty(q)
                ? list.Take(SearchResultLimit)
                : list.Where(a =>
                    (a.Symbol?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Name?.Contains((query ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ?? false))
                    .Take(SearchResultLimit);
            return filtered.Select(a => new StockSearchResult(a.Symbol ?? "", a.Name ?? "", a.Exchange, a.Class)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alpaca search failed for query {Query}", query);
            return Array.Empty<StockSearchResult>();
        }
    }

    public async Task<StockDetailDto?> GetAssetAsync(string symbol, CancellationToken ct = default)
    {
        var settings = await GetSettingsOrThrowAsync(ct);

        var baseUrl = (settings.BaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://paper-api.alpaca.markets";

        var normalized = symbol.Trim().ToUpperInvariant();
        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(10);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v2/assets/{normalized}");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);

        try
        {
            var response = await client.SendAsync(request, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var asset = await response.Content.ReadFromJsonAsync<AlpacaAssetDto>(JsonOptions, ct);
            return asset == null ? null : new StockDetailDto(asset.Symbol ?? "", asset.Name ?? "", asset.Exchange, asset.Class);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Alpaca get asset failed for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<decimal?> GetLatestPriceAsync(string symbol, CancellationToken ct = default)
    {
        var settings = await GetSettingsOrThrowAsync(ct);
        var normalized = symbol.Trim().ToUpperInvariant();
        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(10);
        var dataApiBase = "https://data.alpaca.markets";
        try
        {
            var price = await GetLatestPriceFromQuoteAsync(client, settings, dataApiBase, normalized, ct);
            if (price.HasValue) return price;
            price = await GetLatestPriceFromSnapshotAsync(client, settings, dataApiBase, normalized, ct);
            return price;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Alpaca latest price failed for {Symbol}", symbol);
            return null;
        }
    }

    private static async Task<decimal?> GetLatestPriceFromQuoteAsync(HttpClient client, AlpacaSettings settings, string dataApiBase, string normalized, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{dataApiBase}/v2/stocks/{normalized}/quotes/latest");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);
        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        var quote = await response.Content.ReadFromJsonAsync<AlpacaQuoteDto>(JsonOptions, ct);
        if (quote?.Quote == null) return null;
        var ap = quote.Quote.AskPrice;
        var bp = quote.Quote.BidPrice;
        if (ap.HasValue && bp.HasValue) return (ap.Value + bp.Value) / 2;
        return ap ?? bp;
    }

    private static async Task<decimal?> GetLatestPriceFromSnapshotAsync(HttpClient client, AlpacaSettings settings, string dataApiBase, string normalized, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{dataApiBase}/v2/stocks/{normalized}/snapshot");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);
        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        var snapshot = await response.Content.ReadFromJsonAsync<AlpacaSnapshotDto>(JsonOptions, ct);
        var q = snapshot?.LatestQuote;
        if (q == null) return null;
        var ap = q.AskPrice;
        var bp = q.BidPrice;
        if (ap.HasValue && bp.HasValue) return (ap.Value + bp.Value) / 2;
        return ap ?? bp;
    }

    public async Task<IReadOnlyList<Candle>> GetBarsAsync(string symbol, string timeframe, DateTime start, DateTime end, CancellationToken ct = default)
    {
        var settings = await GetSettingsOrThrowAsync(ct);
        var normalized = symbol.Trim().ToUpperInvariant();
        var tf = string.IsNullOrWhiteSpace(timeframe) ? "1Day" : timeframe.Trim();
        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(15);
        var dataApiBase = "https://data.alpaca.markets";
        var startStr = start.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var endStr = end.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var request = new HttpRequestMessage(HttpMethod.Get, $"{dataApiBase}/v2/stocks/{normalized}/bars?timeframe={Uri.EscapeDataString(tf)}&start={Uri.EscapeDataString(startStr)}&end={Uri.EscapeDataString(endStr)}&limit=1000");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);

        try
        {
            var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return Array.Empty<Candle>();
            var wrapper = await response.Content.ReadFromJsonAsync<AlpacaBarsResponse>(JsonOptions, ct);
            if (wrapper?.Bars == null || wrapper.Bars.Count == 0) return Array.Empty<Candle>();

            return wrapper.Bars
                .Select(b => new Candle(
                    b.T,
                    b.O,
                    b.H,
                    b.L,
                    b.C,
                    b.V,
                    normalized,
                    tf))
                .OrderBy(c => c.Time)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Alpaca bars failed for {Symbol}", symbol);
            return Array.Empty<Candle>();
        }
    }

    private sealed class AlpacaAssetDto
    {
        public string? Symbol { get; set; }
        public string? Name { get; set; }
        public string? Exchange { get; set; }
        [JsonPropertyName("class")]
        public string? Class { get; set; }
    }

    private sealed class AlpacaQuoteDto
    {
        [JsonPropertyName("quote")]
        public AlpacaQuoteInner? Quote { get; set; }
    }

    private sealed class AlpacaSnapshotDto
    {
        [JsonPropertyName("latestQuote")]
        public AlpacaQuoteInner? LatestQuote { get; set; }
    }

    private sealed class AlpacaQuoteInner
    {
        [JsonPropertyName("ap")]
        public decimal? AskPrice { get; set; }
        [JsonPropertyName("bp")]
        public decimal? BidPrice { get; set; }
    }

    private sealed class AlpacaBarsResponse
    {
        [JsonPropertyName("bars")]
        public List<AlpacaBarDto> Bars { get; set; } = new();
    }

    private sealed class AlpacaBarDto
    {
        [JsonPropertyName("t")]
        public DateTime T { get; set; }
        [JsonPropertyName("o")]
        public decimal O { get; set; }
        [JsonPropertyName("h")]
        public decimal H { get; set; }
        [JsonPropertyName("l")]
        public decimal L { get; set; }
        [JsonPropertyName("c")]
        public decimal C { get; set; }
        [JsonPropertyName("v")]
        public decimal V { get; set; }
    }
}

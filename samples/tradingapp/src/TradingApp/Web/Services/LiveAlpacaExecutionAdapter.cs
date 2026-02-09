using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bipins.Trading.Domain;
using Bipins.Trading.Execution;
using Microsoft.Extensions.Logging;
using TradingApp.Application;

namespace TradingApp.Web.Services;

/// <summary>
/// Live execution adapter that submits real orders to the Alpaca Trading API
/// using the user's stored credentials (via IAlpacaSettingsRepository).
/// </summary>
public sealed class LiveAlpacaExecutionAdapter : IExecutionAdapter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAlpacaSettingsRepository _settingsRepository;
    private readonly IFillReceiver? _fillReceiver;
    private readonly ILogger<LiveAlpacaExecutionAdapter> _logger;

    public LiveAlpacaExecutionAdapter(
        IHttpClientFactory httpClientFactory,
        IAlpacaSettingsRepository settingsRepository,
        IFillReceiver? fillReceiver,
        ILogger<LiveAlpacaExecutionAdapter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsRepository = settingsRepository;
        _fillReceiver = fillReceiver;
        _logger = logger;
    }

    public void Submit(OrderIntent intent)
    {
        SubmitAsync(intent).GetAwaiter().GetResult();
    }

    public async Task SubmitAsync(OrderIntent intent, CancellationToken cancellationToken = default)
    {
        if (!intent.Quantity.HasValue || intent.Quantity.Value <= 0)
        {
            _logger.LogWarning("Order intent has invalid quantity, skipping: {Symbol}", intent.Symbol);
            return;
        }

        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings == null || string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            _logger.LogError("Alpaca credentials not configured; cannot submit live order for {Symbol}", intent.Symbol);
            return;
        }

        var baseUrl = (settings.BaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://api.alpaca.markets";

        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(15);

        var orderBody = new AlpacaOrderRequest
        {
            Symbol = intent.Symbol,
            Qty = intent.Quantity.Value.ToString("G"),
            Side = intent.Side == OrderSide.Buy ? "buy" : "sell",
            Type = MapOrderType(intent.OrderType),
            TimeInForce = MapTimeInForce(intent.TimeInForce),
            LimitPrice = intent.OrderType is OrderType.Limit or OrderType.StopLimit ? intent.LimitPrice?.ToString("G") : null,
            StopPrice = intent.OrderType is OrderType.Stop or OrderType.StopLimit ? intent.StopPrice?.ToString("G") : null,
            ClientOrderId = intent.ClientOrderId
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/orders");
        request.Headers.Add("APCA-API-KEY-ID", settings.ApiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", settings.ApiSecret);
        request.Content = JsonContent.Create(orderBody, options: JsonOptions);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Alpaca order submission failed ({StatusCode}): {Error}", response.StatusCode, errorBody);
                return;
            }

            var orderResult = await response.Content.ReadFromJsonAsync<AlpacaOrderResponse>(JsonOptions, cancellationToken);
            _logger.LogInformation("Live order submitted: {OrderId} {Side} {Qty} {Symbol}",
                orderResult?.Id, intent.Side, intent.Quantity, intent.Symbol);

            if (orderResult != null && orderResult.Status == "filled" && _fillReceiver != null)
            {
                var fillPrice = orderResult.FilledAvgPrice ?? intent.LimitPrice ?? 0m;
                var fillQty = orderResult.FilledQty ?? intent.Quantity.Value;
                var fill = new Fill(
                    intent.Symbol,
                    orderResult.FilledAt ?? DateTime.UtcNow,
                    intent.Side,
                    fillQty,
                    fillPrice,
                    0m,
                    intent.ClientOrderId);
                _fillReceiver.OnFill(fill);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit live order for {Symbol}", intent.Symbol);
        }
    }

    private static string MapOrderType(OrderType orderType) => orderType switch
    {
        OrderType.Market => "market",
        OrderType.Limit => "limit",
        OrderType.Stop => "stop",
        OrderType.StopLimit => "stop_limit",
        _ => "market"
    };

    private static string MapTimeInForce(TimeInForce tif) => tif switch
    {
        TimeInForce.Day => "day",
        TimeInForce.GTC => "gtc",
        TimeInForce.IOC => "ioc",
        TimeInForce.FOK => "fok",
        _ => "gtc"
    };

    private sealed class AlpacaOrderRequest
    {
        public string Symbol { get; set; } = "";
        public string Qty { get; set; } = "";
        public string Side { get; set; } = "";
        public string Type { get; set; } = "";
        [JsonPropertyName("time_in_force")]
        public string TimeInForce { get; set; } = "";
        [JsonPropertyName("limit_price")]
        public string? LimitPrice { get; set; }
        [JsonPropertyName("stop_price")]
        public string? StopPrice { get; set; }
        [JsonPropertyName("client_order_id")]
        public string? ClientOrderId { get; set; }
    }

    private sealed class AlpacaOrderResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        [JsonPropertyName("filled_avg_price")]
        public decimal? FilledAvgPrice { get; set; }
        [JsonPropertyName("filled_qty")]
        public decimal? FilledQty { get; set; }
        [JsonPropertyName("filled_at")]
        public DateTime? FilledAt { get; set; }
    }
}

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
    
    static LiveAlpacaExecutionAdapter()
    {
        JsonOptions.Converters.Add(new StringOrDecimalConverter());
    }

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
            var errorMsg = $"Invalid order quantity: {intent.Quantity?.ToString() ?? "null"}";
            _logger.LogWarning("Order intent has invalid quantity, skipping: {Symbol}", intent.Symbol);
            throw new ArgumentException(errorMsg, nameof(intent));
        }

        var settings = await _settingsRepository.GetAsync(cancellationToken);
        if (settings == null || string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            var errorMsg = "Alpaca credentials not configured. Please configure your Alpaca API credentials in Settings.";
            _logger.LogError("Alpaca credentials not configured; cannot submit live order for {Symbol}", intent.Symbol);
            throw new InvalidOperationException(errorMsg);
        }

        var baseUrl = (settings.BaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) baseUrl = "https://api.alpaca.markets";

        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(15);

        // Extract expiration date from Metrics if present (for GTD orders)
        DateTime? expirationDate = null;
        if (intent.Metrics != null && intent.Metrics.TryGetValue("expiration_date_ticks", out var ticks))
        {
            expirationDate = new DateTime((long)ticks);
        }

        var orderBody = new AlpacaOrderRequest
        {
            Symbol = intent.Symbol,
            Qty = intent.Quantity.Value.ToString("G"),
            Side = intent.Side == OrderSide.Buy ? "buy" : "sell",
            Type = MapOrderType(intent.OrderType),
            TimeInForce = MapTimeInForce(intent.TimeInForce),
            LimitPrice = intent.OrderType is OrderType.Limit or OrderType.StopLimit ? intent.LimitPrice?.ToString("G") : null,
            StopPrice = intent.OrderType is OrderType.Stop or OrderType.StopLimit ? intent.StopPrice?.ToString("G") : null,
            ClientOrderId = intent.ClientOrderId,
            ExpirationDate = expirationDate
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
                throw new InvalidOperationException($"Alpaca order submission failed ({response.StatusCode}): {errorBody}");
            }

            // Read JSON manually to handle filled_qty which can be string or number
            var jsonText = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Alpaca order response JSON: {Json}", jsonText);
            
            AlpacaOrderResponse orderResult;
            try
            {
                using var doc = JsonDocument.Parse(jsonText, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                var root = doc.RootElement;
                
                // Parse filled_qty first with extra care
                decimal? filledQty = null;
                if (root.TryGetProperty("filled_qty", out var qtyProp))
                {
                    try
                    {
                        filledQty = ParseDecimal(qtyProp);
                        _logger.LogDebug("Parsed filled_qty: {Value} (Type: {Kind})", filledQty, qtyProp.ValueKind);
                    }
                    catch (Exception qtyEx)
                    {
                        _logger.LogWarning(qtyEx, "Failed to parse filled_qty, will use null. Raw value: {Raw}", qtyProp.GetRawText());
                        filledQty = null;
                    }
                }
                
                // Parse filled_avg_price
                decimal? filledAvgPrice = null;
                if (root.TryGetProperty("filled_avg_price", out var priceProp))
                {
                    try
                    {
                        filledAvgPrice = ParseDecimal(priceProp);
                    }
                    catch (Exception priceEx)
                    {
                        _logger.LogWarning(priceEx, "Failed to parse filled_avg_price, will use null. Raw value: {Raw}", priceProp.GetRawText());
                        filledAvgPrice = null;
                    }
                }
                
                orderResult = new AlpacaOrderResponse
                {
                    Id = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    Status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null,
                    FilledAvgPrice = filledAvgPrice,
                    FilledQty = filledQty,
                    FilledAt = root.TryGetProperty("filled_at", out var dateProp) && dateProp.ValueKind != JsonValueKind.Null
                        ? (DateTime.TryParse(dateProp.GetString(), out var dt) ? dt : (DateTime?)null)
                        : null
                };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Failed to parse Alpaca order response JSON: {Json}", jsonText);
                throw new InvalidOperationException($"Failed to parse Alpaca order response: {jsonEx.Message}", jsonEx);
            }
            catch (Exception parseEx)
            {
                _logger.LogError(parseEx, "Unexpected error parsing Alpaca order response JSON: {Json}", jsonText);
                throw new InvalidOperationException($"Failed to parse Alpaca order response: {parseEx.Message}", parseEx);
            }
            
            _logger.LogInformation("Live order submitted: {OrderId} {Side} {Qty} {Symbol}",
                orderResult.Id, intent.Side, intent.Quantity, intent.Symbol);

            if (orderResult.Status == "filled" && _fillReceiver != null)
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
            throw; // Re-throw to let the controller handle the error
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

    private static decimal? ParseDecimal(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
            return null;
        
        if (element.ValueKind == JsonValueKind.String)
        {
            var str = element.GetString();
            if (string.IsNullOrWhiteSpace(str))
                return null;
            if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }
        
        if (element.ValueKind == JsonValueKind.Number)
        {
            try
            {
                return element.GetDecimal();
            }
            catch
            {
                // If GetDecimal fails, try parsing as string
                var str = element.GetRawText();
                if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
                    return result;
                return null;
            }
        }
        
        return null;
    }

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
        [JsonPropertyName("expiration_date")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? ExpirationDate { get; set; }
    }

    private sealed class AlpacaOrderResponse
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        [JsonPropertyName("filled_avg_price")]
        public decimal? FilledAvgPrice { get; set; }
        [JsonPropertyName("filled_qty")]
        [JsonConverter(typeof(StringOrDecimalConverter))]
        public decimal? FilledQty { get; set; }
        [JsonPropertyName("filled_at")]
        public DateTime? FilledAt { get; set; }
    }

    public sealed class StringOrDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    return null;
                if (decimal.TryParse(str, out var result))
                    return result;
                return null;
            }
            
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDecimal();
            
            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}

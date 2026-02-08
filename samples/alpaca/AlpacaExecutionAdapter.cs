using Alpaca.Markets;
using Bipins.Trading.Domain;
using Microsoft.Extensions.Logging;
using AlpacaOrderType = Alpaca.Markets.OrderType;
using AlpacaTimeInForce = Alpaca.Markets.TimeInForce;
using AlpacaOrderSide = Alpaca.Markets.OrderSide;

namespace Bipins.Trading.Execution.Alpaca;

/// <summary>
/// Alpaca broker implementation of IExecutionAdapter.
/// Handles order submission and fill reporting for live trading.
/// </summary>
public sealed class AlpacaExecutionAdapter : IExecutionAdapter, IDisposable
{
    private readonly IAlpacaTradingClient _tradingClient;
    private readonly IFillReceiver? _fillReceiver;
    private readonly ILogger<AlpacaExecutionAdapter>? _logger;
    private readonly Dictionary<string, string> _orderIdMap = new(); // ClientOrderId -> Alpaca OrderId

    public AlpacaExecutionAdapter(
        IAlpacaTradingClient tradingClient,
        IFillReceiver? fillReceiver = null,
        ILogger<AlpacaExecutionAdapter>? logger = null)
    {
        _tradingClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
        _fillReceiver = fillReceiver;
        _logger = logger;
    }

    public void Submit(OrderIntent intent)
    {
        SubmitAsync(intent).GetAwaiter().GetResult();
    }

    public async Task SubmitAsync(OrderIntent intent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!intent.Quantity.HasValue || intent.Quantity.Value <= 0)
            {
                _logger?.LogWarning("Order intent has invalid quantity: {Intent}", intent);
                return;
            }

            var orderRequest = MapToAlpacaOrder(intent);
            var order = await _tradingClient.PostOrderAsync(orderRequest, cancellationToken);

            if (!string.IsNullOrEmpty(intent.ClientOrderId))
            {
                _orderIdMap[intent.ClientOrderId] = order.OrderId.ToString();
            }

            _logger?.LogInformation("Order submitted: {OrderId} for {Symbol} {Side} {Quantity}",
                order.OrderId, intent.Symbol, intent.Side, intent.Quantity);

            // Handle immediate fills (market orders)
            if (order.OrderStatus == OrderStatus.Filled && _fillReceiver != null)
            {
                var fill = MapToFill(order, intent);
                _fillReceiver.OnFill(fill);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to submit order: {Intent}", intent);
            throw;
        }
    }

    private NewOrderRequest MapToAlpacaOrder(OrderIntent intent)
    {
        var side = intent.Side == Domain.OrderSide.Buy ? AlpacaOrderSide.Buy : AlpacaOrderSide.Sell;
        var orderType = MapOrderType(intent.OrderType);
        var timeInForce = MapTimeInForce(intent.TimeInForce);

        // Use fractional shares if quantity is less than 1, otherwise use whole shares
        var quantity = intent.Quantity!.Value;
        var orderQuantity = quantity < 1m 
            ? OrderQuantity.Fractional(quantity)
            : OrderQuantity.Notional(quantity);

        var request = new NewOrderRequest(
            intent.Symbol,
            orderQuantity,
            side,
            orderType,
            timeInForce)
        {
            ClientOrderId = intent.ClientOrderId
        };

        if (intent.LimitPrice.HasValue)
            request.LimitPrice = intent.LimitPrice.Value;

        if (intent.StopPrice.HasValue)
            request.StopPrice = intent.StopPrice.Value;

        return request;
    }

    private AlpacaOrderType MapOrderType(Domain.OrderType intentType)
    {
        return intentType switch
        {
            Domain.OrderType.Market => AlpacaOrderType.Market,
            Domain.OrderType.Limit => AlpacaOrderType.Limit,
            Domain.OrderType.Stop => AlpacaOrderType.Stop,
            Domain.OrderType.StopLimit => AlpacaOrderType.StopLimit,
            _ => AlpacaOrderType.Market
        };
    }

    private AlpacaTimeInForce MapTimeInForce(Domain.TimeInForce intentTif)
    {
        return intentTif switch
        {
            Domain.TimeInForce.GTC => AlpacaTimeInForce.Gtc,
            Domain.TimeInForce.IOC => AlpacaTimeInForce.Ioc,
            Domain.TimeInForce.FOK => AlpacaTimeInForce.Fok,
            Domain.TimeInForce.Day => AlpacaTimeInForce.Day,
            _ => AlpacaTimeInForce.Gtc
        };
    }

    private Fill MapToFill(IOrder order, OrderIntent intent)
    {
        // Use filled quantity if available, otherwise use order quantity
        var filledQty = order.FilledQuantity != 0 ? order.FilledQuantity : order.Quantity;
        var fillPrice = order.AverageFillPrice ?? (order.LimitPrice ?? 0m);
        var fillTime = order.CreatedAtUtc ?? DateTime.UtcNow;
        
        return new Fill(
            order.Symbol,
            fillTime,
            intent.Side,
            (decimal)filledQty,
            (decimal)fillPrice,
            0m, // Commission not available in IOrder interface
            intent.ClientOrderId);
    }

    public void Dispose()
    {
        _tradingClient?.Dispose();
    }
}

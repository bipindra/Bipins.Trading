using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Execution;

public sealed class PaperExecutionAdapter : IExecutionAdapter
{
    private readonly IFillReceiver? _receiver;
    private readonly decimal _slippagePercent;
    private readonly decimal _feePercent;

    public PaperExecutionAdapter(IFillReceiver? receiver, decimal slippagePercent = 0, decimal feePercent = 0)
    {
        _receiver = receiver;
        _slippagePercent = slippagePercent;
        _feePercent = feePercent;
    }

    public void Submit(OrderIntent intent)
    {
        if (!intent.Quantity.HasValue || intent.Quantity.Value <= 0) return;
        var price = intent.OrderType == OrderType.Market
            ? (intent.LimitPrice ?? 0)
            : (intent.LimitPrice ?? intent.StopPrice ?? 0);
        if (price <= 0) return;

        var sign = intent.Side == OrderSide.Buy ? 1m : -1m;
        var slippage = price * (_slippagePercent / 100m) * sign;
        var fillPrice = price + slippage;
        var notional = fillPrice * intent.Quantity.Value;
        var fees = notional * (_feePercent / 100m);

        var fill = new Fill(
            intent.Symbol,
            intent.Time,
            intent.Side,
            intent.Quantity.Value,
            fillPrice,
            fees,
            intent.ClientOrderId);

        _receiver?.OnFill(fill);
    }

    public Task SubmitAsync(OrderIntent intent, CancellationToken cancellationToken = default)
    {
        Submit(intent);
        return Task.CompletedTask;
    }
}

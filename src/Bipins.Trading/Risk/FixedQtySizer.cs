using Bipins.Trading.Domain;

namespace Bipins.Trading.Risk;

public sealed class FixedQtySizer : IPositionSizer
{
    private readonly decimal _quantity;

    public FixedQtySizer(decimal quantity) => _quantity = quantity;

    public OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null)
    {
        var qty = intent.Quantity ?? _quantity;
        return intent with { Quantity = qty };
    }
}

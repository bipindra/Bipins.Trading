using Bipins.Trading.Domain;

namespace Bipins.Trading.Risk;

public sealed class FixedDollarSizer : IPositionSizer
{
    private readonly decimal _dollarAmount;

    public FixedDollarSizer(decimal dollarAmount) => _dollarAmount = dollarAmount;

    public OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null)
    {
        if (currentPrice <= 0) return intent;
        var qty = intent.Quantity ?? (_dollarAmount / currentPrice);
        return intent with { Quantity = qty };
    }
}

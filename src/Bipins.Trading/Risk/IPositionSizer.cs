using Bipins.Trading.Domain;

namespace Bipins.Trading.Risk;

public interface IPositionSizer
{
    OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null);
}

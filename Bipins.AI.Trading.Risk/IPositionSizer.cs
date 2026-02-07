using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Risk;

public interface IPositionSizer
{
    OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null);
}

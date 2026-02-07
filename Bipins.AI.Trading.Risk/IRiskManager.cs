using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Domain.Events;

namespace Bipins.AI.Trading.Risk;

public interface IRiskManager
{
    RiskDecisionEvent Approve(OrderIntent intent, PortfolioState portfolio, object? context = null);
}

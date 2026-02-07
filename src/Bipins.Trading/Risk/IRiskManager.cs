using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;

namespace Bipins.Trading.Risk;

public interface IRiskManager
{
    RiskDecisionEvent Approve(OrderIntent intent, PortfolioState portfolio, object? context = null);
}

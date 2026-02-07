using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Engine;

public interface IPortfolioService
{
    void Apply(Fill fill);
    PortfolioState GetState();
    void Reset(decimal initialCash);
}

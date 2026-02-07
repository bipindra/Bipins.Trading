using Bipins.Trading.Domain;

namespace Bipins.Trading.Engine;

public interface IPortfolioService
{
    void Apply(Fill fill);
    PortfolioState GetState();
    void Reset(decimal initialCash);
}

using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Execution;

public interface IFillReceiver
{
    void OnFill(Fill fill);
}

public interface IExecutionAdapter
{
    void Submit(OrderIntent intent);
    Task SubmitAsync(OrderIntent intent, CancellationToken cancellationToken = default);
}

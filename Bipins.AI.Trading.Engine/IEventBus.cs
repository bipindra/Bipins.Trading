using Bipins.AI.Trading.Domain.Events;

namespace Bipins.AI.Trading.Engine;

public interface IEventBus
{
    void Publish(ITradingEvent evt);
    void Subscribe<T>(Action<T> handler) where T : ITradingEvent;
}

using Bipins.Trading.Domain.Events;

namespace Bipins.Trading.Engine;

public interface IEventBus
{
    void Publish(ITradingEvent evt);
    void Subscribe<T>(Action<T> handler) where T : ITradingEvent;
}

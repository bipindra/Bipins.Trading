using System.Collections.Concurrent;
using Bipins.Trading.Domain.Events;

namespace Bipins.Trading.Engine;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, object> _handlers = new();

    public void Publish(ITradingEvent evt)
    {
        var t = evt.GetType();
        if (_handlers.TryGetValue(t, out var listObj) && listObj is List<Delegate> list)
        {
            foreach (var h in list)
                try { h.DynamicInvoke(evt); } catch { }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : ITradingEvent
    {
        var t = typeof(T);
        var list = (List<Delegate>)_handlers.GetOrAdd(t, _ => new List<Delegate>());
        void Wrapper(ITradingEvent e) => handler((T)e);
        lock (list) { list.Add((Delegate)(Action<ITradingEvent>)Wrapper); }
    }
}

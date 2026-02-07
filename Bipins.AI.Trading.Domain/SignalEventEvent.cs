using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct SignalEventEvent(DateTime Time, SignalEvent Signal) : ITradingEvent
{
    public string Type => "Signal";
}

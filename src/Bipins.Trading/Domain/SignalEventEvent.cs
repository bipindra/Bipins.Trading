using Bipins.Trading.Domain;

namespace Bipins.Trading.Domain.Events;

public readonly record struct SignalEventEvent(DateTime Time, SignalEvent Signal) : ITradingEvent
{
    public string Type => "Signal";
}

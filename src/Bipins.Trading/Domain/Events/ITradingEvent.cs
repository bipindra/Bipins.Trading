namespace Bipins.Trading.Domain.Events;

public interface ITradingEvent
{
    DateTime Time { get; }
    string Type { get; }
}

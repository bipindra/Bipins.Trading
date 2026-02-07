namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct ErrorEvent(DateTime Time, string Message, Exception? Exception = null) : ITradingEvent
{
    public string Type => "Error";
}

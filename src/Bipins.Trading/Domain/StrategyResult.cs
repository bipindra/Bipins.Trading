using Bipins.Trading.Domain.Events;

namespace Bipins.Trading.Domain;

public sealed class StrategyResult
{
    public IReadOnlyList<SignalEvent> Signals { get; init; } = Array.Empty<SignalEvent>();
    public IReadOnlyList<OrderIntent> Orders { get; init; } = Array.Empty<OrderIntent>();
    public IReadOnlyList<DiagnosticEvent> Diagnostics { get; init; } = Array.Empty<DiagnosticEvent>();
}

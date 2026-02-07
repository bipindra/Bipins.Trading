namespace Bipins.AI.Trading.Indicators.Abstractions;

/// <summary>
/// Stateful indicator that maintains internal buffers (e.g. EMAs, ring buffers).
/// Reset() clears state so the same instance can be reused on a new series.
/// </summary>
public interface IStatefulIndicator
{
    /// <summary>Clears internal state. After Reset, the next Update/Compute starts fresh.</summary>
    void Reset();
}

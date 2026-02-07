namespace Bipins.Trading.Indicators.Models;

/// <summary>
/// Base contract for indicator output. Enables charting and strategy code to consume any indicator result uniformly.
/// </summary>
public interface IIndicatorResult
{
    /// <summary>Number of numeric values in this result (1 for single, 3 for bands, etc.).</summary>
    int ValueCount { get; }

    /// <summary>Gets the value at the given index (0-based).</summary>
    double GetValue(int index);

    /// <summary>Whether this result is valid (e.g. after warmup).</summary>
    bool IsValid { get; }
}

namespace Bipins.AI.Trading.Indicators.Models;

/// <summary>
/// Result for single-value indicators (e.g. RSI, ATR).
/// </summary>
/// <param name="Value">The computed value.</param>
/// <param name="IsValid">True if the value is valid (e.g. after warmup).</param>
public readonly record struct SingleValueResult(double Value, bool IsValid = true) : IIndicatorResult
{
    /// <inheritdoc />
    public int ValueCount => 1;

    /// <inheritdoc />
    public double GetValue(int index) => index == 0 ? Value : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>Invalid result (e.g. before warmup).</summary>
    public static SingleValueResult Invalid => new(0, false);
}

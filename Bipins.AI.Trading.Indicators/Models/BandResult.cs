namespace Bipins.AI.Trading.Indicators.Models;

/// <summary>
/// Result for band indicators (e.g. Bollinger: Upper, Middle, Lower).
/// </summary>
/// <param name="Upper">Upper band value.</param>
/// <param name="Middle">Middle line value.</param>
/// <param name="Lower">Lower band value.</param>
/// <param name="IsValid">True if the result is valid.</param>
public readonly record struct BandResult(double Upper, double Middle, double Lower, bool IsValid = true) : IIndicatorResult
{
    /// <inheritdoc />
    public int ValueCount => 3;

    /// <inheritdoc />
    public double GetValue(int index) => index switch
    {
        0 => Upper,
        1 => Middle,
        2 => Lower,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    /// <summary>Invalid result.</summary>
    public static BandResult Invalid => new(0, 0, 0, false);
}

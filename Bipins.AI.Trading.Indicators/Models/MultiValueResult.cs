namespace Bipins.AI.Trading.Indicators.Models;

/// <summary>
/// Result for multi-value indicators (e.g. MACD: Macd, Signal, Histogram).
/// Uses a fixed-size buffer to avoid allocations; values are copied.
/// </summary>
public readonly struct MultiValueResult : IIndicatorResult
{
    private readonly double _v0, _v1, _v2, _v3, _v4;
    private readonly int _count;
    private readonly bool _isValid;

    /// <summary>Creates a 1-value result.</summary>
    public MultiValueResult(double v0, bool isValid = true)
    {
        _v0 = v0; _v1 = _v2 = _v3 = _v4 = 0;
        _count = 1;
        _isValid = isValid;
    }

    /// <summary>Creates a 2-value result.</summary>
    public MultiValueResult(double v0, double v1, bool isValid = true)
    {
        _v0 = v0; _v1 = v1; _v2 = _v3 = _v4 = 0;
        _count = 2;
        _isValid = isValid;
    }

    /// <summary>Creates a 3-value result (e.g. MACD).</summary>
    public MultiValueResult(double v0, double v1, double v2, bool isValid = true)
    {
        _v0 = v0; _v1 = v1; _v2 = v2; _v3 = _v4 = 0;
        _count = 3;
        _isValid = isValid;
    }

    /// <summary>Creates a 4-value result.</summary>
    public MultiValueResult(double v0, double v1, double v2, double v3, bool isValid = true)
    {
        _v0 = v0; _v1 = v1; _v2 = v2; _v3 = v3; _v4 = 0;
        _count = 4;
        _isValid = isValid;
    }

    /// <summary>Creates a 5-value result (e.g. Ichimoku).</summary>
    public MultiValueResult(double v0, double v1, double v2, double v3, double v4, bool isValid = true)
    {
        _v0 = v0; _v1 = v1; _v2 = v2; _v3 = v3; _v4 = v4;
        _count = 5;
        _isValid = isValid;
    }

    /// <inheritdoc />
    public int ValueCount => _count;

    /// <inheritdoc />
    public bool IsValid => _isValid;

    /// <inheritdoc />
    public double GetValue(int index)
    {
        if (index < 0 || index >= _count) throw new ArgumentOutOfRangeException(nameof(index));
        return index switch { 0 => _v0, 1 => _v1, 2 => _v2, 3 => _v3, 4 => _v4, _ => 0 };
    }

    /// <summary>Invalid result (1 value, not valid).</summary>
    public static MultiValueResult Invalid() => new(0, false);
}

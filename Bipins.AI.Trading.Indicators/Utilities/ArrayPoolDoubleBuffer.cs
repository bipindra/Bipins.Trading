using System.Buffers;

namespace Bipins.AI.Trading.Indicators.Utilities;

/// <summary>
/// Rents a double array from ArrayPool and returns it on Dispose. Use in batch Compute to avoid allocations.
/// </summary>
public sealed class ArrayPoolDoubleBuffer : IDisposable
{
    private double[]? _buffer;
    private readonly int _length;

    /// <summary>Rented buffer. Only valid until Dispose.</summary>
    public Span<double> Span => _buffer.AsSpan(0, _length);

    /// <summary>Length of the buffer.</summary>
    public int Length => _length;

    public ArrayPoolDoubleBuffer(int minimumLength)
    {
        _length = minimumLength;
        _buffer = ArrayPool<double>.Shared.Rent(minimumLength);
    }

    /// <summary>Returns the buffer to the pool.</summary>
    public void Dispose()
    {
        if (_buffer != null)
        {
            ArrayPool<double>.Shared.Return(_buffer);
            _buffer = null;
        }
    }
}

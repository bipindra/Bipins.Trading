namespace Bipins.Trading.Indicators.Utilities;

/// <summary>
/// Fixed-capacity ring buffer for double values. Used for period-N windows in indicators without allocations.
/// </summary>
public sealed class RingBufferDouble
{
    private readonly double[] _buffer;
    private int _index;
    private int _count;

    /// <summary>Capacity (period length).</summary>
    public int Capacity => _buffer.Length;

    /// <summary>Number of values currently in the buffer (at most Capacity).</summary>
    public int Count => _count;

    /// <summary>True if buffer has been filled at least once (Count == Capacity).</summary>
    public bool IsFull => _count == _buffer.Length;

    public RingBufferDouble(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _buffer = new double[capacity];
    }

    /// <summary>Adds a value; overwrites oldest when full.</summary>
    public void Add(double value)
    {
        _buffer[_index] = value;
        _index = (_index + 1) % _buffer.Length;
        if (_count < _buffer.Length) _count++;
    }

    /// <summary>Gets the value at relative index (0 = oldest in current window, Count-1 = newest).</summary>
    public double this[int relativeIndex]
    {
        get
        {
            if (relativeIndex < 0 || relativeIndex >= _count)
                throw new ArgumentOutOfRangeException(nameof(relativeIndex));
            int pos = _count < _buffer.Length
                ? relativeIndex
                : (_index + relativeIndex) % _buffer.Length;
            return _buffer[pos];
        }
    }

    /// <summary>Copies the current window (oldest to newest) into the destination span.</summary>
    public void CopyTo(Span<double> destination)
    {
        int n = Math.Min(_count, destination.Length);
        for (int i = 0; i < n; i++)
            destination[i] = this[i];
    }

    /// <summary>Clears the buffer (count = 0).</summary>
    public void Clear()
    {
        _index = 0;
        _count = 0;
    }

    /// <summary>Sum of all values in the buffer.</summary>
    public double Sum()
    {
        double sum = 0;
        for (int i = 0; i < _count; i++) sum += this[i];
        return sum;
    }

    /// <summary>Mean of values in the buffer.</summary>
    public double Mean() => _count == 0 ? 0 : Sum() / _count;
}

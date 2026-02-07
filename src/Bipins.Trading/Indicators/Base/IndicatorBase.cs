using System.Collections.ObjectModel;
using Bipins.Trading.Indicators.Abstractions;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Base;

/// <summary>
/// Base class for all indicators. Implements batch and streaming APIs; subclasses override Update and optionally Reset.
/// </summary>
/// <typeparam name="TResult">Result type (SingleValueResult, MultiValueResult, BandResult).</typeparam>
public abstract class IndicatorBase<TResult> : IIndicator, IStreamingIndicator<TResult>, IBatchIndicator<TResult>, IStatefulIndicator
    where TResult : struct, IIndicatorResult
{
    private readonly List<IndicatorParameter> _parameters = new();
    private int _updateCount;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public IReadOnlyList<IndicatorParameter> Parameters => new ReadOnlyCollection<IndicatorParameter>(_parameters);

    /// <inheritdoc />
    public int WarmupPeriod { get; }

    /// <summary>Number of candles processed so far (since last Reset).</summary>
    protected int UpdateCount => _updateCount;

    /// <summary>True if enough candles have been processed to produce valid output.</summary>
    protected bool IsWarmedUp => _updateCount >= WarmupPeriod;

    protected IndicatorBase(string name, string description, int warmupPeriod)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        WarmupPeriod = warmupPeriod >= 0 ? warmupPeriod : 0;
    }

    /// <summary>Adds a parameter description. Call from constructor.</summary>
    protected void AddParameter(string name, string defaultValue, string description)
    {
        _parameters.Add(new IndicatorParameter(name, defaultValue, description));
    }

    /// <summary>Updates state with one candle and returns the current result.</summary>
    public TResult Update(Candle candle)
    {
        TResult result = ComputeNext(candle);
        IncrementUpdateCount();
        return result;
    }

    /// <summary>Override: compute one step from current candle and internal state. Base calls this then increments count.</summary>
    protected abstract TResult ComputeNext(Candle candle);

    /// <inheritdoc />
    public virtual void Reset()
    {
        _updateCount = 0;
    }

    /// <inheritdoc />
    public IReadOnlyList<TResult> Compute(IReadOnlyList<Candle> candles)
    {
        Reset();
        var results = new List<TResult>(candles.Count);
        for (int i = 0; i < candles.Count; i++)
            results.Add(Update(candles[i]));
        return results;
    }

    /// <inheritdoc />
    public int Compute(ReadOnlySpan<Candle> candles, Span<TResult> results)
    {
        if (results.Length < candles.Length)
            throw new ArgumentException("Results span must have length at least candles.Length.", nameof(results));
        Reset();
        for (int i = 0; i < candles.Length; i++)
            results[i] = Update(candles[i]);
        return candles.Length;
    }

    /// <summary>Called by derived implementations after processing a candle; increments internal count.</summary>
    protected void IncrementUpdateCount()
    {
        _updateCount++;
    }
}

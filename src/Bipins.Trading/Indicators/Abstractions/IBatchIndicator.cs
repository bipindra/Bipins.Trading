using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Abstractions;

/// <summary>
/// Indicator that supports batch computation over a full series of candles.
/// Returns a result per candle (or per valid output index) without requiring prior streaming state.
/// </summary>
/// <typeparam name="TResult">Single result type (e.g. SingleValueResult).</typeparam>
public interface IBatchIndicator<TResult> : IIndicator
    where TResult : struct, IIndicatorResult
{
    /// <summary>Computes the indicator over all candles. Result count matches input count; results before warmup are invalid.</summary>
    /// <param name="candles">Full series of candles (historical or in-memory).</param>
    /// <returns>One result per candle; early entries may be invalid until WarmupPeriod is reached.</returns>
    IReadOnlyList<TResult> Compute(IReadOnlyList<Candle> candles);

    /// <summary>Computes the indicator over a span of candles. Zero-allocation when using stack or pool-backed buffers.</summary>
    /// <param name="candles">Span of candles.</param>
    /// <param name="results">Span to write results into; must have length at least candles.Length.</param>
    /// <returns>Number of valid results written (candles.Length; invalid results still written with IsValid=false).</returns>
    int Compute(ReadOnlySpan<Candle> candles, Span<TResult> results);
}

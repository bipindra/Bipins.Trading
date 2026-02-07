using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Abstractions;

/// <summary>
/// Indicator that supports incremental (streaming) updates: one candle at a time.
/// Each call to Update(candle) advances internal state and returns the current result.
/// </summary>
/// <typeparam name="TResult">Result type (e.g. SingleValueResult, MultiValueResult, BandResult).</typeparam>
public interface IStreamingIndicator<out TResult> : IIndicator
    where TResult : struct, IIndicatorResult
{
    /// <summary>Updates the indicator with the next candle and returns the current result.</summary>
    /// <param name="candle">The next OHLCV candle.</param>
    /// <returns>Current indicator result (may be invalid before warmup).</returns>
    TResult Update(Candle candle);
}

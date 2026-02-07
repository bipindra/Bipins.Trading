using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Abstractions;

/// <summary>
/// Generic indicator contract: computes output of type TOutput from input of type TInput.
/// Supports batch Compute over candles and/or single-step computation.
/// </summary>
/// <typeparam name="TInput">Input type (e.g. Candle, ReadOnlySpan of Candle).</typeparam>
/// <typeparam name="TOutput">Output type (e.g. SingleValueResult, MultiValueResult, or a collection of results).</typeparam>
public interface IIndicator<in TInput, TOutput> : IIndicator
{
    /// <summary>Computes output for the given input (e.g. single candle for streaming, or full series for batch).</summary>
    TOutput Compute(TInput input);
}

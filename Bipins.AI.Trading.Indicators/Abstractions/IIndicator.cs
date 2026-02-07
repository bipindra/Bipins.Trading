using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Abstractions;

/// <summary>
/// Base contract for all technical indicators. Broker-agnostic, charting-agnostic, execution-agnostic.
/// </summary>
public interface IIndicator
{
    /// <summary>Short name of the indicator (e.g. "RSI", "MACD").</summary>
    string Name { get; }

    /// <summary>Human-readable description and typical use.</summary>
    string Description { get; }

    /// <summary>Read-only description of parameters (name, default, meaning).</summary>
    IReadOnlyList<IndicatorParameter> Parameters { get; }

    /// <summary>Minimum number of candles required before the indicator produces valid output.</summary>
    int WarmupPeriod { get; }
}

/// <summary>
/// Describes a single indicator parameter (name, default value, description).
/// </summary>
/// <param name="Name">Parameter name.</param>
/// <param name="DefaultValue">Default value (string representation for display).</param>
/// <param name="Description">What the parameter controls.</param>
public readonly record struct IndicatorParameter(string Name, string DefaultValue, string Description);

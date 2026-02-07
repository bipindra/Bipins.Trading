# Bipins.AI.Trading

A .NET algorithmic trading library that provides a **technical indicator library** (100+ indicators with batch and streaming APIs) and a **broker-agnostic strategy engine** for backtesting and paper trading. The engine consumes normalized market data, uses the indicators library, and emits chart-ready signals and execution-ready order intents.

---

## What Is Implemented

### 1. Technical indicators library (Bipins.AI.Trading.Indicators)

- **Abstractions:** `IIndicator`, `IStreamingIndicator<T>`, `IBatchIndicator<T>`, `IStatefulIndicator`; result types `SingleValueResult`, `MultiValueResult`, `BandResult`.
- **Base:** `IndicatorBase<TResult>` with warmup, `Update(Candle)`, `Compute(ReadOnlySpan, Span)`, `Reset()`.
- **Utilities:** `RingBufferDouble`, `ArrayPoolDoubleBuffer`, `SpanMath` for low-allocation and span-based math.
- **Models:** `Candle` (OHLCV), `Quote`, `TradeTick`, `OrderBookLevel`, `IIndicatorResult`.

### 2. Strategy engine (broker-agnostic)

- **Domain:** Core models (`Candle` with Symbol/Timeframe, `Position`, `OrderIntent`, `SignalEvent`, `Fill`, `PortfolioState`, `StrategyState`), enums (`PositionSide`, `OrderSide`, `OrderType`, `TimeInForce`, `SignalType`), event envelope (`ITradingEvent`, `SignalEventEvent`, `OrderIntentEvent`, `FillEvent`, `PositionChangedEvent`, `RiskDecisionEvent`, `DiagnosticEvent`, `ErrorEvent`), strategy API (`IStrategy`, `StrategyContext`, `StrategyResult`, `StrategyWarmup`, `IIndicatorProvider`, `IndicatorKey`).
- **Engine:** In-process `IEventBus` / `InMemoryEventBus`, `IndicatorProvider` (per-key cache, mapping domain candles to indicator candles), `IMarketDataFeed` / `HistoricalCandleFeed`, `IPortfolioService` / `PortfolioService`, `BacktestRunner` and `BacktestFillReceiver`, `BacktestConfig` / `BacktestResult`.
- **Strategies:** `StrategyBase`, `EmaCrossoverStrategy`, `RsiMeanReversionStrategy`, `BreakoutDonchianStrategy`.
- **Risk:** `IRiskManager`, `CompositeRiskManager`, `IRiskPolicy`, `MaxPositionsPolicy`, `MaxDailyLossPolicy`; `IPositionSizer`, `FixedQtySizer`, `FixedDollarSizer`, `PercentEquitySizer`, `AtrRiskSizer`.
- **Execution:** `IExecutionAdapter`, `IFillReceiver`, `PaperExecutionAdapter` (simulated fills, optional slippage/fees).
- **Persistence:** `IStateStore`, `InMemoryStateStore`, `JsonFileStateStore`.
- **Charting:** `IChartSink`, `JsonFileChartSink` (signal export to JSON).
- **CLI:** Console app with `backtest [config.json]` (loads config, runs backtest, exports signals).

---

## Projects

| Project | Purpose |
|--------|---------|
| **Bipins.AI.Trading.Indicators** | Indicator library: abstractions, base, models, utilities, and all indicator categories below. |
| **Bipins.AI.Trading.Indicators.Tests** | xUnit tests for indicators (known-value tests for SMA, EMA, RSI, MACD, Bollinger Bands, models). |
| **Bipins.AI.Trading.Domain** | Core domain models, enums, events, and strategy/indicator-provider contracts. |
| **Bipins.AI.Trading.Engine** | Event bus, indicator provider, market data feed, portfolio service, backtest runner. |
| **Bipins.AI.Trading.Strategies** | Strategy base and example strategies (EMA crossover, RSI mean reversion, Donchian breakout). |
| **Bipins.AI.Trading.Risk** | Risk policies and position sizers. |
| **Bipins.AI.Trading.Execution.Abstractions** | Execution and fill-receiver interfaces. |
| **Bipins.AI.Trading.Execution.Paper** | Paper execution adapter. |
| **Bipins.AI.Trading.Persistence** | In-memory and JSON file state stores. |
| **Bipins.AI.Trading.Charting.Abstractions** | Chart sink interface and JSON file export. |
| **Bipins.AI.Trading.Cli** | Console application for backtest (and config-driven runs). |
| **Bipins.AI.Trading.Tests** | Engine, backtest runner, indicator provider, and risk tests. |

---

## Indicator Categories and Names

Indicators are grouped by category. Each supports **streaming** (`Update(Candle)`) and **batch** (`Compute(IReadOnlyList<Candle>)` or `Compute(ReadOnlySpan<Candle>, Span<TResult>)`).

### Moving Averages (14)

SMA, EMA, WMA, SMMA, RMA, HMA, DEMA, TEMA, KAMA, VWMA, VMA, ZLMA, ALMA, FRAMA.

### Momentum (14)

RSI, Stochastic, StochRSI, CCI, ROC, Momentum, Williams %R, TSI, Ultimate Oscillator, RVI, CMO, Connors RSI, Fisher Transform, DPO.

### Trend (14)

MACD, MACD Histogram, ADX, Directional Movement, Aroon, Aroon Oscillator, Parabolic SAR, SuperTrend, Ichimoku Cloud, TRIX, Vortex Indicator, Linear Regression, Linear Regression Intercept, Linear Regression Slope.

### Volatility (10)

ATR, Bollinger Bands, Bollinger Band Width, Keltner Channels, Donchian Channels, Standard Deviation, Chaikin Volatility, Historical Volatility, Mass Index, Volatility Ratio, Envelope.

### Volume (11)

OBV, ADL, CMF, MFI, Volume Oscillator, EMV, Force Index, NVI, PVI, VWAP, Rolling VWAP, Volume Profile.

### Support & Resistance (9)

Pivot Points: Classic, Fibonacci, Camarilla, Woodie, DeMark; Fibonacci Retracement; Fibonacci Extension; Linear Regression Channel; Price Channel.

### Oscillators (5)

Awesome Oscillator, Accelerator Oscillator, Price Oscillator, PPO, Detrended Oscillator.

### Market Structure (5)

Fractals, ZigZag, Swing High/Low, Trend Strength Index, Market Facilitation Index.

### Advanced (6)

Kaufman Efficiency Ratio, Elder Ray Index, Elder Impulse System, Choppiness Index, TTM Squeeze, Z-Score Indicator.

### Statistical (9)

Mean, Variance, Standard Deviation, Z-Score, Skewness, Kurtosis, Correlation, Covariance, Beta.

### Price Transform (5)

Heikin Ashi, Median Price, Typical Price, Weighted Close, Log Returns.

---

## Strategies (Examples)

- **EmaCrossoverStrategy** – Fast/slow EMA crossover; entry/exit long and short; uses `IndicatorKey.Ema(period)` with optional previous-bar key for crossover detection.
- **RsiMeanReversionStrategy** – RSI oversold/overbought thresholds (configurable); long on oversold, short on overbought; exit on opposite extreme.
- **BreakoutDonchianStrategy** – Donchian channel breakout (upper/lower); entries and exits based on channel penetration; uses `GetMulti` for upper/middle/lower.

All use `IIndicatorProvider` with caching (e.g. `ctx.Indicators.Get(key, () => Compute(...))`).

---

## Techniques and Design

- **Broker-agnostic:** No broker or exchange dependency; domain uses symbols, timeframes, and generic order/signal/fill models.
- **Batch and streaming:** Indicators implement both `Compute(span, span)` (zero-allocation where possible) and `Update(Candle)`.
- **Stateful indicators:** `Reset()` for reusing on a new series; warmup period before valid results.
- **Performance:** `Span<T>`, `ArrayPool`, ring buffers; no LINQ in hot paths in the indicator library.
- **Deterministic backtest:** Single-threaded bar processing, fixed symbol/timeframe order; fill receiver applies fills and collects trades; equity curve and trade list produced.
- **Event-driven pipeline:** Event bus for `ITradingEvent`; sinks (e.g. chart export) subscribe to signals and optional other events.

---

## Testing Strategy

- **Indicators (Bipins.AI.Trading.Indicators.Tests):** Known-value or reference tests for core indicators and models: `SmaTests`, `EmaTests`, `RsiTests`, `MacdTests`, `BollingerBandsTests`, `ModelsTests`; shared `TestData` where needed.
- **Engine and strategies (Bipins.AI.Trading.Tests):**  
  - **IndicatorProviderTests:** Cache behavior (e.g. compute once per key, `ClearCache`).  
  - **BacktestRunnerTests:** Run backtest with synthetic candles; assert equity curve and optional determinism (same config/data ⇒ same result).  
  - **RiskTests:** Risk policies (e.g. `MaxPositionsPolicy` reject when at limit) and position sizers (`FixedQtySizer`, `PercentEquitySizer`).

Run all tests: `dotnet test`.

---

## Build and Run

**Requirements:** .NET 8 SDK.

```bash
dotnet restore
dotnet build
dotnet test
```

**CLI backtest:**

```bash
dotnet run --project Bipins.AI.Trading.Cli -- backtest config.json
```

Config (e.g. `config.json`) can include: `Symbols`, `Timeframes`, `Start`, `End`, `InitialCash`, `WarmupBars`, `DataPath` (optional CSV), `ChartExportPath` (default `signals.json`). If `DataPath` is missing, synthetic daily data is generated for the configured range.

---

## Architecture (High Level)

- **Indicators:** Standalone library; input is OHLCV `Candle` (no symbol/timeframe in the indicator type); output is `SingleValueResult`, `MultiValueResult`, or `BandResult`.
- **Domain:** Defines `Candle` with Symbol/Timeframe for the engine; strategy context exposes `IIndicatorProvider`; engine implements provider and maps domain candles to indicator candles when calling the indicator library.
- **Pipeline (backtest):** Feed → candles per symbol/timeframe → strategy runner (with warmup) → risk → sizer → execution (paper) → fill receiver → portfolio update and event bus → chart/other sinks.

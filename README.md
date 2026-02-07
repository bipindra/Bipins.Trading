# Bipins.AI.Trading

.NET algorithmic trading library: technical indicators and a broker-agnostic strategy engine.

## Build & test

```bash
dotnet restore
dotnet build
dotnet test
```

## Projects

- **Bipins.AI.Trading.Indicators** – 100+ indicators (batch and streaming).
- **Bipins.AI.Trading.Indicators.Tests** – Indicator tests.
- **Bipins.AI.Trading.Domain** – Core models and strategy contracts.
- **Bipins.AI.Trading.Engine** – Backtest runner, event bus, indicator provider.
- **Bipins.AI.Trading.Strategies** – Example strategies (EMA crossover, RSI, Donchian).
- **Bipins.AI.Trading.Risk** – Risk policies and position sizers.
- **Bipins.AI.Trading.Execution.*** – Execution abstractions and paper trading.
- **Bipins.AI.Trading.Persistence** – State stores.
- **Bipins.AI.Trading.Charting.Abstractions** – Chart/signal export.
- **Bipins.AI.Trading.Cli** – Console app: `dotnet run --project Bipins.AI.Trading.Cli -- backtest config.json`
- **Bipins.AI.Trading.Tests** – Engine/strategy/risk tests.

## Requirements

.NET 8 SDK.

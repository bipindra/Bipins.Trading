# Alpaca Live Trading Sample

This sample demonstrates how to use Bipins.Trading with Alpaca for live trading.

## Architecture

```mermaid
graph TB
    subgraph "Alpaca API"
        AC[Alpaca Trading Client]
        ADC[Alpaca Data Client]
        ASC[Alpaca Streaming Client]
    end

    subgraph "Market Data Layer"
        MDF[AlpacaMarketDataFeed]
        MDF -->|Historical Data| ADC
        MDF -->|Real-time Bars| ASC
    end

    subgraph "Trading Engine"
        LTE[LiveTradingEngine]
        LTE -->|Subscribes| MDF
        LTE -->|Processes| STRAT[Strategies]
        LTE -->|Manages| CB[Candle Buffers]
        LTE -->|Uses| IP[IndicatorProvider]
    end

    subgraph "Strategy Layer"
        STRAT -->|Generates| SIG[Signals]
        STRAT -->|Generates| OI[Order Intents]
    end

    subgraph "Risk Management"
        RM[RiskManager]
        PS[PositionSizer]
        RM -->|Approves/Rejects| OI
        PS -->|Sizes| OI
    end

    subgraph "Execution Layer"
        EA[AlpacaExecutionAdapter]
        EA -->|Submits Orders| AC
        EA -->|Receives Fills| FR[LiveFillReceiver]
    end

    subgraph "Portfolio Management"
        PSVC[PortfolioService]
        FR -->|Updates| PSVC
        PSVC -->|State| LTE
    end

    subgraph "Event System"
        EB[EventBus]
        FR -->|Publishes| EB
        LTE -->|Publishes| EB
        EB -->|Notifies| SUB[Subscribers]
    end

    %% Data Flow
    ASC -->|Real-time Bars| MDF
    MDF -->|Candles| LTE
    LTE -->|Order Intent| RM
    RM -->|Approved Intent| PS
    PS -->|Sized Intent| EA
    EA -->|Order| AC
    AC -->|Fill| FR
    FR -->|Fill Event| EB
    FR -->|Portfolio Update| PSVC
    LTE -->|Signal Event| EB

    style AC fill:#e1f5ff
    style ADC fill:#e1f5ff
    style ASC fill:#e1f5ff
    style LTE fill:#fff4e1
    style STRAT fill:#e8f5e9
    style RM fill:#ffebee
    style EA fill:#f3e5f5
    style PSVC fill:#e0f2f1
    style EB fill:#fff9c4
```

## Prerequisites

1. **Alpaca Account**: Sign up at https://alpaca.markets/
2. **API Credentials**: Get your API Key ID and Secret Key from the Alpaca dashboard
3. **.NET 8 SDK**: Required to run the application

## Setup

### 1. Set Environment Variables

**Windows (PowerShell):**
```powershell
$env:ALPACA_API_KEY_ID="your_api_key_id"
$env:ALPACA_SECRET_KEY="your_secret_key"
```

**Linux/Mac:**
```bash
export ALPACA_API_KEY_ID="your_api_key_id"
export ALPACA_SECRET_KEY="your_secret_key"
```

### 2. Build the Project

```bash
dotnet build samples/alpaca/Bipins.Trading.Samples.Alpaca.csproj
```

### 3. Run

**Paper Trading (default):**
```bash
dotnet run --project samples/alpaca/Bipins.Trading.Samples.Alpaca.csproj
```

**Live Trading (use with caution!):**
```bash
dotnet run --project samples/alpaca/Bipins.Trading.Samples.Alpaca.csproj -- live
```

## Data Flow

```mermaid
sequenceDiagram
    participant Alpaca as Alpaca API
    participant MDF as MarketDataFeed
    participant LTE as LiveTradingEngine
    participant Strategy as Strategy
    participant RM as RiskManager
    participant PS as PositionSizer
    participant EA as ExecutionAdapter
    participant FR as FillReceiver
    participant Portfolio
    participant EB as EventBus

    Alpaca->>MDF: Real-time Bar Updates
    MDF->>LTE: New Candle
    LTE->>LTE: Update Candle Buffer
    LTE->>Strategy: OnBar(Context)
    Strategy->>Strategy: Calculate Indicators
    Strategy->>LTE: Return Signals & Orders
    
    alt Entry Signal
        LTE->>EB: Publish SignalEvent
        LTE->>RM: Approve Order
        RM->>LTE: RiskDecision
        alt Approved
            LTE->>PS: Size Position
            PS->>LTE: Sized Order Intent
            LTE->>EA: Submit Order
            EA->>Alpaca: Post Order
            Alpaca->>EA: Order Confirmation
            EA->>FR: OnFill
            FR->>Portfolio: Apply Fill
            FR->>EB: Publish FillEvent
        end
    end
    
    alt Exit Signal
        LTE->>EB: Publish SignalEvent
        LTE->>EA: Submit Exit Order
        EA->>Alpaca: Post Order
        Alpaca->>EA: Order Confirmation
        EA->>FR: OnFill
        FR->>Portfolio: Apply Fill
        FR->>EB: Publish FillEvent
    end
```

## Features

- **Real-time Market Data**: Subscribes to live bar updates from Alpaca
- **Order Execution**: Submits orders through Alpaca API
- **Portfolio Tracking**: Tracks positions and cash in real-time
- **Risk Management**: Enforces position limits and daily loss limits
- **Event-Driven**: Uses event bus for fill and signal notifications

## Configuration

The sample uses:
- **Strategies**: EMA Crossover and RSI Mean Reversion
- **Risk Limits**: Max 5 positions, 5% daily loss limit
- **Position Sizing**: 10% of equity per position
- **Symbols**: SPY and QQQ (configurable in code)

## Important Notes

⚠️ **Paper Trading First**: Always test with paper trading before using live trading.

⚠️ **Risk Management**: The sample includes basic risk management, but you should customize it for your needs.

⚠️ **Market Hours**: Alpaca only executes orders during market hours. Orders placed outside market hours will be queued.

## Customization

To customize the sample:

1. **Change Symbols**: Modify `GetStrategySymbols()` in `LiveTradingEngine.cs`
2. **Change Strategies**: Update the strategies array in `Program.cs`
3. **Adjust Risk Limits**: Modify risk policies in `Program.cs`
4. **Change Position Sizing**: Update `PercentEquitySizer` parameters

## Troubleshooting

- **Connection Errors**: Verify your API credentials are correct
- **No Orders**: Check that market is open and you have sufficient buying power
- **Data Issues**: Ensure your Alpaca account has market data access

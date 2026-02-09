# Trading App Sample

Mobile-first SPA: Vue 3 (Vite) + .NET 8 ASP.NET Core MVC. UI with watchlist, stock search via Alpaca API, settings, price alerts, and notifications. Alerts are evaluated in the background; when a trigger fires, a one-shot notification is created and optional desktop notifications can be shown.

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm (for frontend build and optional dev server)

## Quick start

Target framework: **.NET 8**. Requires .NET 8 SDK.

### 1. Build

From the **solution root** (`samples/tradingapp`):

```bash
dotnet build src/TradingApp/TradingApp.csproj
```

If the build fails with `NETSDK1045` (Bipins.Trading targets .NET 9+), use:  
`dotnet build src/TradingApp/TradingApp.csproj /p:BuildNet8Only=true`

### 2. Run

```bash
cd src/TradingApp
dotnet run
```

### 3. Apply database migrations (optional)

Migrations run automatically on startup. To apply manually:

```bash
cd src/TradingApp
dotnet ef database update
```

### 4. (Optional) Configure Alpaca API

For stock search and details, configure Alpaca credentials. **Do not commit secrets.**

**Development (user-secrets):**

```bash
cd src/TradingApp
dotnet user-secrets set "Alpaca:ApiKey" "YOUR_KEY"
dotnet user-secrets set "Alpaca:ApiSecret" "YOUR_SECRET"
dotnet user-secrets set "Alpaca:BaseUrl" "https://paper-api.alpaca.markets"
```

Or configure via the **Settings** tab in the app (stored in SQLite, secret encrypted with Data Protection).

**Production:** Use environment variables (e.g. `Alpaca__ApiKey`, `Alpaca__ApiSecret`, `Alpaca__BaseUrl`).

- API: http://localhost:5000  
- Swagger (Development): http://localhost:5000/swagger

### 5. Serve the SPA

**Option A – Production (built SPA served by backend)**  
Build the Vue app once, then run the backend:

```bash
cd src/TradingApp/Web/ClientApp
npm install
npm run build
cd ../..
dotnet run
```

Open http://localhost:5000 — the backend serves the built SPA from `Web/ClientApp/dist`.

**Option B – Development (Vite dev server + proxy)**  
Terminal 1 – backend:

```bash
cd src/TradingApp
dotnet run
```

Terminal 2 – frontend:

```bash
cd src/TradingApp/Web/ClientApp
npm install
npm run dev
```

Open http://localhost:5173 — Vite proxies `/api` to the backend (port 5000). Hot reload works.

## Project layout

The sample is a **single project** (`TradingApp`) with folders for layering:

- **Domain/** – Entities (WatchlistItem, AlpacaSettings, Alert, AlertType, Notification)
- **Application/** – IAlpacaService, AlpacaService, DTOs, IAlertEngine (stub), app services
- **Infrastructure/** – EF Core (SQLite), migrations, SecretEncryption, repositories
- **Web/** – Program.cs, API controllers, middleware, hosted services; **Web/ClientApp** – Vue 3 + Vite + Vue Router + Pinia (mobile-first, aqua/white theme)

## API summary

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/stocks/search?q= | Search assets (Alpaca) |
| GET | /api/stocks/{symbol} | Stock detail |
| GET | /api/watchlist | List watchlist |
| POST | /api/watchlist | Add symbol |
| DELETE | /api/watchlist/{symbol} | Remove |
| GET | /api/settings/alpaca | Masked Alpaca settings |
| POST | /api/settings/alpaca | Save Alpaca settings |
| GET | /api/alerts?symbol= | List alerts for symbol |
| POST | /api/alerts | Create alert |
| GET | /api/notifications?limit=&unreadOnly= | Recent notifications |
| PATCH | /api/notifications/{id}/read | Mark notification read |

**Error responses:** `400` and `500` return JSON `{ "message": "...", "errors": ["..."]? }`. Validation uses the same shape.

## Background alert watcher

A hosted service runs every 60 seconds: it loads all **untriggered** alerts (where `TriggeredAt` is null), fetches latest price per symbol from the Alpaca Data API, and for **RSI-based** alerts also fetches recent daily bars from Alpaca. Evaluation uses **Bipins.Trading**:

- **Price alerts**: PriceAbove / PriceBelow (payload = threshold).
- **Indicator alerts**: RsiOversold (default RSI ≤ 30) and RsiOverbought (default RSI ≥ 70), using Bipins.Trading’s **Rsi** indicator on the last 60 days of daily bars. Payload is optional: period (default 14), or `period,oversold,overbought` (e.g. `14,30,70`).

When a condition hits, a **Notification** is created, the alert’s `TriggeredAt` is set (one-shot), a **SignalEvent** is published to Bipins.Trading’s **IEventBus** (if registered), and optional **IExecutionEngine** runs (e.g. paper order via Bipins.Trading). The frontend **Notifications** tab polls GET `/api/notifications` and can enable desktop notifications for new items.

## Execution on trigger (Bipins.Trading)

When **Trading:ExecuteOnTrigger** is `true` in configuration (e.g. `appsettings.Development.json` or user-secrets), the app registers **IExecutionEngine** with a Bipins.Trading-based implementation. After an alert triggers (notification created, alert marked triggered), the engine submits a **paper order** via Bipins.Trading’s `PaperExecutionAdapter` (Market, 1 share at trigger price; PriceAbove → Buy, PriceBelow → Sell). No fill receiver is registered, so orders are logged only. To enable:

```json
{
  "Trading": {
    "ExecuteOnTrigger": true
  }
}
```

If needed, build with `/p:BuildNet8Only=true` (see step 1).

## TODO – Demonstrating Bipins.Trading library capabilities

The list below is a roadmap of features that would showcase the **Bipins.Trading** library: engines, signals, strategies, indicator selection, risk, execution, and charting. Use it as a backlog or inspiration for extending this sample.

---

### Engines

- [ ] **Backtest runner in-app** – Add an API or UI to run **BacktestRunner** with user-selected symbols, date range, and strategy; use **HistoricalCandleFeed** fed by **IAlpacaService.GetBarsAsync** (e.g. daily bars).
- [ ] **Backtest config API** – Expose `BacktestConfig` (symbols, timeframes, start/end, initial cash, warmup bars, strategy parameters) via REST and persist last-used config in **IStateStore** or app settings.
- [ ] **Live/paper engine** – Run **IStrategy** in real time (e.g. on Alpaca bars or 1m aggregates) with **PaperExecutionAdapter** and **IPortfolioService**; emit **SignalEvent** and **OrderIntentEvent** to **IEventBus**.
- [ ] **Multi-timeframe backtest** – Use **BacktestRunner** with multiple timeframes (e.g. 1Day + 1Hour) and pass per-timeframe **IndicatorKey** so strategies can use higher-timeframe context.
- [ ] **IndicatorProvider in alert flow** – Reuse **IndicatorProvider** (or a scoped cache) when evaluating indicator-based alerts so RSI/MACD/etc. are consistent with backtest and strategy APIs.

---

### Signals

- [ ] **Signal history API** – Persist or expose **SignalEvent** (Strategy, Symbol, Time, SignalType, Price, Reason, Metrics) from backtest runs and from live/paper runs; GET `/api/signals?symbol=&strategy=&from=&to=`.
- [ ] **SignalEventEvent subscription** – Subscribe to **IEventBus** for **SignalEventEvent** in a hosted service or API and push new signals to the client (e.g. SignalR channel) for live strategy feedback.
- [ ] **Signal types in UI** – Display **SignalType** (Hold, EntryLong, ExitLong, EntryShort, ExitShort) and optional **Metrics** (e.g. RSI value at signal) in watchlist, stock detail, or a dedicated “Signals” tab.
- [ ] **Signal-to-order trace** – When execution on trigger is enabled, link **SignalEvent** → **OrderIntentEvent** → **FillEvent** and show the chain in notifications or an activity feed.

---

### Strategies

- [ ] **Strategy selector** – Let the user choose one or more **IStrategy** (e.g. **EmaCrossoverStrategy**, **RsiMeanReversionStrategy**, **BreakoutDonchianStrategy**) for backtest or paper runs; register strategies by name in DI.
- [ ] **Strategy parameters UI** – Expose **StrategyContext.Parameters** (e.g. EMA periods, RSI oversold/overbought, Donchian period) in Settings or backtest config and pass into **BacktestRunner** via **BacktestConfig.StrategyParameters**.
- [ ] **Custom strategy from config** – Support loading a custom strategy (e.g. DLL or script) or a predefined combo (e.g. “EMA crossover + RSI filter”) and run it in **BacktestRunner**.
- [ ] **Strategy warmup** – Respect **IStrategy.Warmup** (RequiredCandleCount) in backtest and in live bar consumption; show “warming up” in UI when bar count is below warmup.
- [ ] **OnTick strategy support** – If a strategy implements **IStrategy.OnTick**, feed **TradeTick** from Alpaca or simulated ticks in paper mode and invoke **OnTick** alongside **OnBar**.

---

### Indicator selection and alerts

- [ ] **Indicator picker UI** – Allow users to pick one or more indicators (e.g. RSI, MACD, Bollinger Bands, ATR, Stochastic) with parameters (period, timeframe) and optional thresholds for alerts or strategy inputs.
- [ ] **IndicatorKey and IIndicatorProvider** – Use **IndicatorKey.Rsi(period)**, **IndicatorKey.Ema(period)**, **IndicatorKey.Macd(fast, slow, signal)**, **IndicatorKey.Atr(period)**, **IndicatorKey.Donchian(period)** (and custom keys) with **IIndicatorProvider.Get** / **GetMulti** in strategies and in alert evaluation.
- [ ] **More alert types** – Add alerts driven by **Bipins.Trading** indicators: MACD cross, Bollinger Band touch, Stochastic overbought/oversold, ATR-based stop, ADX trend strength, etc., using the same pattern as RSI alerts.
- [ ] **Multi-indicator alerts** – Combine conditions (e.g. RSI &lt; 30 and price &gt; 200-day SMA) using the indicator provider and bar history; store composite rule in alert payload.
- [ ] **Indicator values in watchlist** – Optionally show last-computed RSI, MACD, or other indicator value next to price on the watchlist (batch compute on load or via SignalR push).
- [ ] **100+ indicators** – Wire more library indicators (Momentum: CCI, CMO, StochRsi, Williams %R; Trend: ADX, Ichimoku, ParabolicSar, SuperTrend; Volatility: BollingerBands, KeltnerChannels; Volume: OBV, MFI, VWAP; etc.) into alert config or strategy parameters.

---

### Risk

- [ ] **IRiskManager in backtest** – Run **BacktestRunner** with **CompositeRiskManager** and configurable **IRiskPolicy** (e.g. **MaxPositionsPolicy**, **MaxDailyLossPolicy**) so backtest respects risk limits.
- [ ] **Position sizers** – Let the user choose **IPositionSizer**: **FixedQtySizer**, **FixedDollarSizer**, **PercentEquitySizer**, or **AtrRiskSizer** (ATR-based size) for backtest and paper execution; expose size params in config.
- [ ] **Risk policy settings** – API or UI to set max open positions, max daily loss, max position size as % of equity; persist and pass into **CompositeRiskManager**.
- [ ] **RiskDecisionEvent** – Subscribe to **IEventBus** for **RiskDecisionEvent** and surface “order rejected by risk” in notifications or logs when **IRiskManager** blocks an intent.

---

### Execution and fills

- [ ] **IFillReceiver and fill history** – Register **IFillReceiver** (e.g. **TradingAppFillReceiver**) and persist **FillEvent** (or a DTO) to DB; expose GET `/api/fills` for activity and PnL.
- [ ] **Order intent history** – Persist or log **OrderIntentEvent** (before execution) and link to **FillEvent**; show “pending” vs “filled” in UI.
- [ ] **Paper vs live adapter** – Keep **PaperExecutionAdapter** for dev/demo; add a path to plug a live **IExecutionAdapter** (e.g. Alpaca) when the user opts in, with clear “paper” vs “live” mode in settings.
- [ ] **Slippage and fees** – Use **PaperExecutionAdapter** options (if exposed) for slippage/fee simulation in backtest and paper runs.

---

### Events and event bus

- [ ] **IEventBus dashboard** – Optional debug view that subscribes to **IEventBus** and streams recent **ITradingEvent** (Signal, OrderIntent, Fill, PositionChanged, RiskDecision, Diagnostic, Error) to the client (e.g. SignalR or SSE).
- [ ] **Event persistence** – Optionally persist events to **IStateStore** or a log table for replay or audit.
- [ ] **PositionChangedEvent** – On **FillEvent**, portfolio updates; subscribe to **PositionChangedEvent** to push position updates to the client (e.g. “You now have 10 shares of AAPL”).

---

### Charting and export

- [ ] **IChartSink integration** – After a backtest run, pass **IChartSink** (e.g. **JsonFileChartSink**) into **BacktestRunner** so signals and equity curve are exported to JSON for charting tools or custom UI.
- [ ] **Equity curve API** – **BacktestResult** (or runner) can expose equity curve; add GET `/api/backtest/result` or `/api/backtest/equity-curve` for the last run (or by run id) and plot in the SPA.
- [ ] **Signal overlay** – In stock detail or a dedicated backtest view, show candles plus signal markers (e.g. EntryLong/ExitLong) from **BacktestResult** or from **JsonFileChartSink** output.
- [ ] **Export backtest report** – Export **BacktestResult** (PnL, drawdown, trade count, etc.) as JSON or CSV for external analysis.

---

### Persistence and state

- [ ] **IStateStore for app state** – Use **JsonFileStateStore** or **InMemoryStateStore** for backtest config, last-selected strategy, or user preferences (e.g. default indicators) and load on startup.
- [ ] **Strategy state** – If strategies use **StrategyState** or custom state, persist via **IStateStore** keyed by strategy + symbol so state survives restarts in paper mode.

---

### Frontend and UX

- [ ] **Backtest page** – Dedicated page: symbol(s), date range, strategy dropdown, “Run backtest” button; show summary (PnL, trades, max drawdown) and link to equity curve or signal list.
- [ ] **Indicators panel** – On stock detail or watchlist, “Add indicator” with dropdown (RSI, MACD, BB, etc.) and period; display last value or mini sparkline.
- [ ] **Alerts builder** – UI to add alert with “Condition”: Price above/below, RSI oversold/overbought, or (future) MACD cross, BB touch, etc., with payload for parameters.
- [ ] **Paper trading mode toggle** – Settings: “Paper trading” on/off; when on, all execution goes through **PaperExecutionAdapter** and fills are shown as “Paper” in activity.
- [ ] **Notifications for all event types** – Extend notifications to include not only alert triggers but also fills, risk rejections, and (optional) signal events.

---

### Integration notes (existing and future)

- **IAlertEngine** (Application): interface for executing when an alert is created; the in-repo flow uses **AlertTriggerEvaluator** and the background watcher for evaluation.
- **IExecutionEngine** (Application): optional execution when an alert triggers; implementation uses Bipins.Trading’s **PaperExecutionAdapter** (Buy for PriceAbove/RsiOversold, Sell for PriceBelow/RsiOverbought).
- **IFillReceiver** (Bipins.Trading): when execution on trigger is enabled, **TradingAppFillReceiver** is registered: it logs paper fills and publishes **FillEvent** to **IEventBus**.
- **IEventBus** (Bipins.Trading): **InMemoryEventBus** is registered; on alert trigger a **SignalEventEvent** is published; on paper fill a **FillEvent** is published.
- **IAlpacaService.GetBarsAsync**: returns **Bipins.Trading.Domain.Candle** list for indicator-based alerts (Alpaca Data API v2 bars).
- **Bipins.Trading:** RSI alerts use **Indicators.Momentum.Rsi**; execution uses **Execution.PaperExecutionAdapter**. More indicator types and strategies can be wired the same way.

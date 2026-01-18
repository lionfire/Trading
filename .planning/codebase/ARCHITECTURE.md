# Architecture

**Analysis Date:** 2026-01-18

## Pattern Overview

**Overall:** Layered Architecture with DataFlow Processing Pipeline

**Key Characteristics:**
- Clean separation between abstractions and implementations
- DataFlow-based input/output system for bots and indicators
- Hierarchical parameter system with optimization support
- Precision-generic numeric types (TPrecision: double/decimal)
- Orleans-based distributed processing for grains
- Reactive programming patterns (ReactiveUI, DynamicData)

## Layers

**Abstractions Layer:**
- Purpose: Define interfaces and contracts for the entire trading system
- Location: `src/LionFire.Trading.Abstractions/`
- Contains: Interfaces (IBot2, IAccount2, IPMarketProcessor), data structures (IKline, TimeFrame), DataFlow components
- Depends on: LionFire.Core libraries
- Used by: All other trading modules

**Automation Layer:**
- Purpose: Bot execution, backtesting, and optimization framework
- Location: `src/LionFire.Trading.Automation/`
- Contains: Bot harnesses, simulation contexts, optimization strategies, backtest batching
- Depends on: Abstractions, Indicators
- Used by: Blazor UI, Orleans workers

**Indicators Layer:**
- Purpose: Technical analysis indicators with historical and realtime harnesses
- Location: `src/LionFire.Trading.Indicators/`
- Contains: Indicator implementations, harnesses for execution, parameter definitions
- Depends on: Abstractions
- Used by: Automation (bots consume indicator outputs)

**Historical Data Layer:**
- Purpose: File-based storage and retrieval of market data
- Location: `src/LionFire.Trading.HistoricalData/`
- Contains: BarsFileSource, BarsService, chunk serialization, data retrieval jobs
- Depends on: Abstractions
- Used by: Automation, Indicators

**Grains Layer (Orleans):**
- Purpose: Distributed actor framework for real-time operations
- Location: `src/LionFire.Trading.Grains.Abstractions/`, `src/LionFire.Trading.Binance.Grains/`
- Contains: Grain interfaces (IBarScraperG, ITickStreamG, IRealtimeBotHarnessG)
- Depends on: Abstractions, Microsoft.Orleans.Sdk
- Used by: Live trading applications

**UI Layer (Blazor):**
- Purpose: Web-based user interface for trading automation
- Location: `src/LionFire.Trading.Automation.Blazor/`
- Contains: Razor components for optimization, bot management, results visualization
- Depends on: Automation, MudBlazor, ScottPlot
- Used by: Web applications

## Data Flow

**Bot Execution Pipeline:**

1. Market data enters via `IPInput` / `IPKlineInput` (DataFlow inputs)
2. InputSlots on parameter objects bind to data sources
3. Bot's `OnBar()` method receives notification when data is ready
4. Bot reads indicator values and current market state
5. Bot executes trades via `IAccount2.ExecuteMarketOrder()`
6. Journal records trade results

**Backtesting Flow:**

1. `OptimizationTask` creates `MultiSimContext` for the optimization run
2. `GridSearchStrategy` enumerates parameter combinations
3. `BacktestQueue` batches backtests for parallel execution
4. `BatchHarness` runs multiple bots over historical data
5. `SimContext<TPrecision>` tracks simulation time and state
6. `BacktestsJournal` persists results to filesystem

**State Management:**
- Simulation state managed by `SimContext<TPrecision>`
- Position tracking via `IObservableCache<IPosition<TPrecision>, int>` (DynamicData)
- Holdings tracked per account via `IObservableCache<IHolding<TPrecision>, string>`

## Key Abstractions

**Bot2 Hierarchy:**
- Purpose: Core bot implementation pattern
- Examples: `src/LionFire.Trading.Automation/Automation/Bots/Bot2.cs`, `BotBase2.cs`, `StandardBot2.cs`
- Pattern: Generic base class `BotBase2<TParameters, TPrecision>` with typed parameters

**Parameter Objects (PBot2):**
- Purpose: Strongly-typed bot configuration with optimization support
- Examples: `src/LionFire.Trading.Automation/Automation/Bots/Parameters/PBot2.cs`, `PBarsBot2.cs`
- Pattern: CRTP pattern `PBot2<TConcrete>` inheriting `PMarketProcessor`

**DataFlow Slots:**
- Purpose: Connect inputs/outputs between processors
- Examples: `src/LionFire.Trading.Abstractions/DataFlow/Slots/InputSlot.cs`
- Pattern: Slot-based binding for composable data pipelines

**Indicator Harness:**
- Purpose: Execute indicators over historical or realtime data
- Examples: `src/LionFire.Trading.Indicators/Indicators/Harnesses/IndicatorHarness.cs`, `HistoricalIndicatorHarness.cs`
- Pattern: Generic harness `IndicatorHarness<TIndicator, TParameters, TInput, TOutput>`

## Entry Points

**Optimization Entry:**
- Location: `src/LionFire.Trading.Automation/Optimization/OptimizationTask.cs`
- Triggers: Web UI via OneShotOptimize, CLI commands
- Responsibilities: Initialize MultiSimContext, run optimization strategy, persist results

**Backtest Batch Entry:**
- Location: `src/LionFire.Trading.Automation/Backtesting2/Batching/BacktestQueue.cs`
- Triggers: OptimizationTask submitting parameter combinations
- Responsibilities: Queue management, parallel execution, progress tracking

**Bot Harness Entry:**
- Location: `src/LionFire.Trading.Automation/Automation/Bots/Execution/BotHarness.cs`
- Triggers: BacktestQueue (batch) or direct instantiation (live)
- Responsibilities: Initialize bot inputs, manage lifecycle, track positions

**Historical Data Entry:**
- Location: `src/LionFire.Trading.HistoricalData/Data/Bars/Service/BarsService.cs`
- Triggers: Indicator harnesses, backtest initialization
- Responsibilities: Load from disk cache, retrieve from exchange if missing

## Error Handling

**Strategy:** Exception-based with validation patterns

**Patterns:**
- `IValidatable` interface with `ValidateOrThrow()` extension method
- `AlreadySetException` for immutable properties set twice
- `NoDataException` for failed data retrieval
- `BotFaultException` for bot execution failures
- Polly resilience pipelines for filesystem retry (`FilesystemRetryPolicy`)

## Cross-Cutting Concerns

**Logging:** Microsoft.Extensions.Logging with ILogger<T> injection. Serilog configured at host level.

**Validation:** Custom `IValidatable` interface with `ValidationContext` accumulating errors. Called via `ValidateOrThrow()`.

**Authentication:** Delegated to external providers. Exchange API keys stored in environment/configuration.

**Dependency Injection:** Heavy use of `Microsoft.Extensions.DependencyInjection`. Each module provides `*HostingX.cs` extension methods:
- `src/LionFire.Trading.Automation/Hosting/AutomationHostingX.cs`
- `src/LionFire.Trading.HistoricalData/Data/Bars/Hosting/HistoricalBarsHostingX.cs`
- `src/LionFire.Trading.Indicators/Hosting/IndicatorsHostingX.cs`

---

*Architecture analysis: 2026-01-18*

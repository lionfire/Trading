# Codebase Structure

**Analysis Date:** 2026-01-18

## Directory Layout

```
/mnt/c/src/Trading/
├── src/                           # Source code (main libraries)
│   ├── LionFire.Trading.Abstractions/    # Core interfaces and contracts
│   ├── LionFire.Trading.Automation/      # Bot execution and optimization
│   ├── LionFire.Trading.Automation.Blazor/ # Web UI components
│   ├── LionFire.Trading.Automation.Bots/ # Bot implementations (proprietary)
│   ├── LionFire.Trading.Indicators/      # Technical indicators
│   ├── LionFire.Trading.HistoricalData/  # Market data storage
│   ├── LionFire.Trading.Grains.Abstractions/ # Orleans grain interfaces
│   ├── LionFire.Trading.Feeds.Binance/   # Binance data feed
│   ├── LionFire.Trading.Feeds.Bybit/     # Bybit data feed
│   └── ...                               # Additional exchange/feature modules
├── tests/                         # Test projects
│   ├── LionFire.Trading.Tests/
│   ├── LionFire.Trading.Automation.Tests/
│   └── LionFire.Trading.Indicators.Tests/
├── benchmarks/                    # Performance benchmarks
├── docs/                          # Documentation
├── .planning/                     # GSD planning documents
│   └── codebase/                  # Codebase analysis docs
├── Directory.Build.props          # MSBuild configuration
├── Directory.Packages.props       # Central package management
└── global.json                    # .NET SDK version pinning
```

## Directory Purposes

**`src/LionFire.Trading.Abstractions/`:**
- Purpose: Foundation interfaces and data structures for entire system
- Contains: Interfaces (IBot2, IAccount2, IKline), DataFlow system, enums, value types
- Key files:
  - `Bots/ITBot.cs` - Bot interface definitions
  - `Accounts/IAccount2.cs` - Account abstraction
  - `DataFlow/IPMarketProcessor.cs` - Market processor interface
  - `DataFlow/Slots/InputSlot.cs` - Input slot system
  - `Structures/Bars/IKline.cs` - OHLCV bar data

**`src/LionFire.Trading.Automation/`:**
- Purpose: Bot execution framework, backtesting, and optimization
- Contains: Bot base classes, harnesses, simulation contexts, optimization strategies
- Key files:
  - `Automation/Bots/Bot2.cs` - Bot base class
  - `Automation/Bots/BotBase2.cs` - Core bot implementation
  - `Automation/Bots/Parameters/PBot2.cs` - Parameter base class
  - `Automation/Bots/Execution/BotHarness.cs` - Bot execution harness
  - `Automation/Simulations/Contexts/SimContext.cs` - Simulation context
  - `Optimization/OptimizationTask.cs` - Optimization orchestrator
  - `Backtesting2/Batching/BacktestQueue.cs` - Batch backtest queue

**`src/LionFire.Trading.Indicators/`:**
- Purpose: Technical analysis indicators
- Contains: Indicator implementations, harnesses, parameter definitions
- Key files:
  - `Indicators/Harnesses/IndicatorHarness.cs` - Base indicator harness
  - `Indicators/Harnesses/HistoricalIndicatorHarness.cs` - Historical data harness
  - `Indicators/Implementations/SimpleMovingAverage.cs` - SMA implementation
  - `Hosting/IndicatorsHostingX.cs` - DI registration

**`src/LionFire.Trading.HistoricalData/`:**
- Purpose: File-based market data storage and retrieval
- Contains: Filesystem sources, serialization, data services
- Key files:
  - `Data/Bars/Filesystem/BarsFileSource.cs` - Local disk data source
  - `Data/Bars/Service/BarsService.cs` - Composite data service
  - `Data/Bars/Filesystem/Serialization/KlineFileDeserializer.cs` - Bar file parser
  - `Data/Bars/Hosting/HistoricalBarsHostingX.cs` - DI registration

**`src/LionFire.Trading.Automation.Blazor/`:**
- Purpose: Blazor Server UI components for trading automation
- Contains: Razor components, view models, charts
- Key files:
  - `Optimization/OneShotOptimize.razor` - Main optimization UI
  - `Optimization/BacktestResults.razor` - Results display
  - `Optimization/OptimizationStatus.razor` - Progress tracking
  - `Bots/Bots.razor` - Bot management
  - `Bots/Bot.razor` - Single bot view

**`src/LionFire.Trading.Grains.Abstractions/`:**
- Purpose: Orleans grain interfaces for distributed operations
- Contains: Grain interfaces for feeds, bots, streaming
- Key files:
  - `Feeds/IBarScraperG.cs` - Bar scraper grain interface
  - `Streaming/Ticks/ITickStreamG.cs` - Tick streaming grain
  - `Bots/IRealtimeBotHarnessG.cs` - Live bot harness grain
  - `User/IUserPreferencesG.cs` - User preferences grain

**`tests/`:**
- Purpose: Unit and integration tests
- Contains: xUnit test projects by feature area
- Key files:
  - `LionFire.Trading.Indicators.Tests/` - Indicator unit tests
  - `LionFire.Trading.Automation.Tests/Optimizing/` - Optimization tests
  - `LionFire.Trading.Automation.Tests/Backtesting/` - Backtest tests

## Key File Locations

**Entry Points:**
- `src/LionFire.Trading.Automation/Optimization/OptimizationTask.cs`: Main optimization entry
- `src/LionFire.Trading.Automation/Backtesting2/Batching/BacktestQueue.cs`: Batch backtest queue
- `src/LionFire.Trading.Automation/Automation/Bots/Execution/BotHarness.cs`: Bot execution

**Configuration:**
- `Directory.Build.props`: MSBuild properties (output paths, versioning)
- `Directory.Packages.props`: Central NuGet package versions
- `global.json`: .NET SDK version (9.0)

**Core Logic:**
- `src/LionFire.Trading.Automation/Automation/Bots/BotBase2.cs`: Bot implementation base
- `src/LionFire.Trading.Automation/Automation/Simulations/MultiSims/MultiSimContext.cs`: Optimization context
- `src/LionFire.Trading.Indicators/Indicators/Harnesses/HistoricalIndicatorHarness.cs`: Indicator execution

**Testing:**
- `tests/LionFire.Trading.Indicators.Tests/`: Indicator verification
- `tests/LionFire.Trading.Automation.Tests/`: Automation integration tests

## Naming Conventions

**Files:**
- Interfaces: `I{Name}.cs` (e.g., `IBot2.cs`, `IAccount2.cs`)
- Parameter classes: `P{Name}.cs` (e.g., `PBot2.cs`, `POptimization.cs`)
- Hosting extensions: `*HostingX.cs` (e.g., `AutomationHostingX.cs`)
- Blazor components: `{Name}.razor` + `{Name}.razor.cs` code-behind
- View models: `{Name}VM.cs` (e.g., `BotVM.cs`, `BotsVM.cs`)

**Directories:**
- Feature-based grouping within projects
- `Hosting/` for DI registration extensions
- `Automation/` for execution-related code
- `Backtesting2/` for v2 backtesting implementation

## Where to Add New Code

**New Bot Type:**
- Parameter class: `src/LionFire.Trading.Automation.Bots/` (or proprietary repo)
- Implementation: Same location, inheriting `Bot2<TParameters, TPrecision>`
- Register in: `BotTypeRegistry` via assembly scanning

**New Indicator:**
- Interface: `src/LionFire.Trading.Abstractions/Indicators/I{Name}.cs`
- Parameters: `src/LionFire.Trading.Indicators/Parameters/P{Name}.cs`
- Implementation: `src/LionFire.Trading.Indicators/Indicators/Implementations/{Name}.cs`
- Tests: `tests/LionFire.Trading.Indicators.Tests/{Name}Tests.cs`

**New UI Component:**
- Razor: `src/LionFire.Trading.Automation.Blazor/{Feature}/{Name}.razor`
- Code-behind: Same location, `{Name}.razor.cs`
- View model (if needed): Same location, `{Name}VM.cs`

**New Exchange Feed:**
- Project: `src/LionFire.Trading.Feeds.{Exchange}/`
- Implement: `IBarScraperG` grain interface
- Historical data retrieval: Implement in `RetrieveHistoricalDataJob`

**New Grain:**
- Interface: `src/LionFire.Trading.Grains.Abstractions/{Feature}/I{Name}G.cs`
- Implementation: Exchange-specific grains project (e.g., Binance.Grains)

**Utilities:**
- Trading utilities: `src/LionFire.Trading.Abstractions/` relevant subdirectory
- Automation utilities: `src/LionFire.Trading.Automation/` relevant subdirectory

## Special Directories

**`z:\build\`:**
- Purpose: Build output (configured via Directory.Build.props)
- Generated: Yes
- Committed: No (symlink to external drive)

**`.planning/`:**
- Purpose: GSD planning and codebase analysis
- Generated: Partially (by mapping commands)
- Committed: Yes

**`openspec/`:**
- Purpose: Change proposals and specifications
- Generated: No
- Committed: Yes

**`benchmarks/`:**
- Purpose: Performance benchmarks using BenchmarkDotNet
- Generated: Reports generated during runs
- Committed: Yes (code and select reports)

---

*Structure analysis: 2026-01-18*

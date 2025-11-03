# Project Context

## Purpose
LionFire Trading is a comprehensive trading automation platform that enables:
- Automated trading bot development and execution
- Strategy backtesting and optimization
- Portfolio management with backtest result tracking
- Multi-workspace organization for different trading strategies
- Real-time monitoring and risk management

The platform supports both paper trading (demo) and live trading with safety controls.

## Tech Stack

### Core Framework
- **.NET 9.0** - Primary runtime and SDK
- **C#** - Main programming language with nullable reference types enabled
- **LionFire Framework** - Custom MVVM/reactive architecture for trading applications

### Frontend
- **Blazor Server (Interactive Server)** - Real-time web UI using SignalR
- **MudBlazor 7.x** - Material Design component library
- **ReactiveUI** - Reactive MVVM framework with property change notifications
- **ReactiveUI.SourceGenerators** - Code generation for reactive properties using `[Reactive]` attribute

### Backend & Services
- **Microsoft Orleans** - Distributed actor framework for scalable grain-based services
- **Consul** - Service discovery and Orleans cluster coordination
- **ASP.NET Core** - Web hosting and HTTP services

### Data & Persistence
- **HJSON** - Human-friendly JSON format for configuration files (root braces omitted by convention)
- **DynamicData** - Reactive collections and observable caches
- **File-based persistence** - HJSON files stored in workspace directories
- **Reactive persistence layer** - `IObservableReaderWriter<TKey, TValue>` for reactive file I/O

### Trading Infrastructure
- **CCXT** - Unified exchange API integration
- **Phemex** - Primary exchange connector (futures trading)
- **Binance** - Secondary exchange support
- **Custom indicator framework** - Technical analysis indicators

### Development Environment
- **Windows 11 + WSL2** - Primary development environment
- **Docker** - Containerization for services
- **Visual Studio 2022** - Primary IDE
- **DevContainers** - Standardized development environments

## Project Conventions

### Code Style

#### File Organization
plan/ - Project planning documents (PRD.md, tasks.md, done.md)
src/ - Main source code
artifacts/ - Build output artifacts
test/ - Test projects
research/ - Temporary test/research projects

#### HJSON Format
- **ALWAYS omit root braces** in HJSON files
- Use `//` comments for documentation
- Indentation: 2 spaces (not tabs)
- Example:
  ```hjson
  // Configuration
  name: MyBot
  enabled: true
  parameters:
  {
    riskPercent: 1.5
  }
  ```

#### Property Patterns
- Use **`[Reactive]` attribute** for auto-save enabled properties:
  ```csharp
  [Reactive]
  private string? _name;  // Generates public Name property with change notifications
  ```
- For read-only computed properties, use standard get-only properties
- File extensions determined by `[Alias]` attribute (e.g., `[Alias("Bot")]` → `.bot.hjson`)

#### Naming Conventions
- Classes: PascalCase (e.g., `Portfolio2`, `BotEntity`)
- Properties: PascalCase
- Private fields with `[Reactive]`: `_camelCase` with underscore prefix
- Interfaces: `I` prefix (e.g., `IObservableReaderWriter`)
- Type parameters: `T` prefix (e.g., `TKey`, `TValue`, `TValueVM`)

### Architecture Patterns

#### Workspace Architecture
- **Multi-workspace support** - Users can have multiple isolated workspaces
- **Workspace-scoped services** - Each workspace has its own `IServiceProvider`
- **Cascading parameters** - Services flow down: UserServices → WorkspaceServices → Components
- **Path structure**: `C:/ProgramData/LionFire/Trading/Users/{Username}/Workspaces/{WorkspaceId}/`

#### MVVM Pattern
- **Model**: Data entities (e.g., `Portfolio2`, `BotEntity`) inherit from `ReactiveObject`
- **ViewModel**: Wrapper VMs (e.g., `Portfolio2VM`, `BotVM`) inherit from `KeyValueVM<TKey, TValue>`
- **View**: Blazor components using `ObservableDataView<TKey, TValue, TValueVM>`

#### Reactive Data Flow
1. **File-based storage** - HJSON files in workspace directories
2. **Observable readers** - `HjsonFsDirectoryReaderRx<TKey, TValue>` watches filesystem
3. **FileSystemWatcher** - Detects external file changes and reloads data
4. **DynamicData caches** - `IObservableCache<TValue, TKey>` holds reactive collections
5. **Autosave** - `AutosaveValueChanges()` subscribes to property changes and writes back
6. **UI binding** - Blazor components subscribe to cache changes via `ItemsChanged` observable

#### Service Registration Patterns
- Use **`AddWorkspaceChildType<T>(recursive, recursionDepth)`** for workspace document types
- `recursive: true, recursionDepth: 1` - Scan immediate subdirectories only (for `[Vos(VosFlags.PreferDirectory)]` types)
- `recursive: false` - Scan root directory only (for flat file storage)
- Workspace services configured via `IWorkspaceServiceConfigurator` pattern

#### File Storage Patterns
- **Flat files**: `BotEntitys/MyBot.bot.hjson` (entity name in filename)
- **Directories**: `Portfolio2s/~New~XXX/portfolio.hjson` (entity name is directory, file is type name)
- Controlled by `[Vos(VosFlags.PreferDirectory)]` attribute

### Testing Strategy
- Unit tests for core business logic
- Integration tests for file I/O and reactive pipelines
- Backtesting framework validates trading strategies against historical data
- Manual testing via Blazor UI with real-time updates

### Git Workflow
- Multiple repositories: `/src/Core` (framework), `/src/Trading` (trading platform), `/src/Trading.Proprietary` (private strategies)
- Commit messages end with: "🤖 Generated with [Claude Code](https://claude.com/claude-code)\nCo-Authored-By: Claude <noreply@anthropic.com>"
- Use `git_win` for push operations (better Windows authentication)

## Domain Context

### Trading Concepts
- **Bot/BotEntity** - Trading strategy configuration with parameters, exchange, symbol, timeframe
- **Portfolio2** - Collection of backtest results grouped for analysis
- **Backtest** - Historical simulation of a trading strategy
- **Optimization** - Parameter tuning via grid search or genetic algorithms
- **Exchange Areas** - Trading venues (e.g., spot, futures, margin)
- **TimeFrame** - Candlestick intervals (e.g., 1m, 5m, 1h, 1d)

### Key Attributes
- **Live vs Paper Trading** - `Live` property determines real money vs demo account
- **Enabled** - Controls whether bot is actively trading
- **Risk Management** - Position limits, loss limits, circuit breakers, kill switches

### File Naming
- Workspace metadata: `{WorkspaceName}.hjson`
- Workspace directory: `{WorkspaceName}/`
- Entity collections: Plural form using `GetPluralName()` (respects `[Alias]` attribute)
  - `Portfolio2` → "Portfolio" (via `[Alias]`) → "Portfolios/" directory
  - `BotEntity` → "Bot" (via `[Alias]`) → "Bots/" directory

## Important Constraints

### Platform Constraints
- **Windows + WSL2** - Development and deployment on Windows with Linux subsystem
- **Path mapping**: `/mnt/c` = `C:`, `/mnt/d` = `D:`, etc.
- **Use `dotnet_win`** instead of `dotnet` when working on Windows filesystem mounts
- **Windows paths** required for Windows executables: `C:\src\...` not `/mnt/c/src/...`

### Performance Considerations
- **Debounced autosave** - 1 second throttle on property changes to avoid excessive disk writes
- **Lazy loading** - Collections load on-demand when UI subscribes
- **FileSystemWatcher** - Enables real-time file change detection with subscription-based lifecycle
- **Recursion depth limits** - Prevents deep directory scanning (`recursionDepth: 1` for portfolios)

### Security & Safety
- **Risk management** - Mandatory stop-loss, position sizing, exposure limits
- **Circuit breakers** - Auto-disable trading on rapid losses or volatility spikes
- **Kill switch** - Emergency stop for all trading activity
- **Separate credentials** - Demo vs live trading account isolation

## External Dependencies

### Exchange APIs
- **CCXT (ccxt.com)** - Unified cryptocurrency exchange API library
- **Phemex API** - Primary futures trading platform
- **Binance API** - Secondary exchange support

### Infrastructure Services
- **Consul** - Orleans clustering and service discovery (cluster: "blue")
- **PostgreSQL** - Risk management database (connection: `trading_risk`)
- **OpenTelemetry** - Distributed tracing and metrics (endpoint: http://localhost:4317)

### NuGet Packages
- ReactiveUI.Blazor
- MudBlazor (Material Design components)
- DynamicData (reactive collections)
- Microsoft.Orleans.* (distributed actors)
- Serilog.* (structured logging)

### Development Tools
- **dotnet-script** - C# scripting for quick tests
- **Inno Setup** - Windows installer creation
- **NuGet** - Package management (local feed: `/mnt/c/LocalNuGetFeed`)

## Key File Locations

### Configuration
- Workspace data: `C:/ProgramData/LionFire/Trading/Users/{Username}/Workspaces/`
- Application settings: `appsettings.json`, `appsettings.local.json`, `appsettings.development.json`
- Environment variables: `/srv/trading/.env`

### Logs
- Application logs: `/src/tp/logs/LionFire.Trading.Silo{timestamp}.log`
- Structured logging via Serilog with daily rotation

### Historical Data
- Base directory: `C:\st\Investing-HistoricalData\`
- Organized by exchange/symbol/timeframe

## Component-Specific Notes

### ObservableDataView Component
- Generic reactive data grid component for workspace entities
- Requires: `DataServiceProvider="WorkspaceServices"` parameter
- Auto-columns based on type reflection, or use explicit `<Columns>` section
- Make `Key` property `Editable="false"` to prevent rename crashes
- Enable creation: `AllowedEditModes=EditMode.All`

### Reactive Property Requirements
For autosave to work, entities MUST:
1. Inherit from `ReactiveObject`
2. Be `partial class`
3. Use `[Reactive]` attribute on properties
4. Include `using ReactiveUI.SourceGenerators;`

Without reactive properties, changes won't trigger autosave!

### Service Registration Pattern
```csharp
services
    .AddWorkspaceChildType<Portfolio2>(recursive: true, recursionDepth: 1)  // Subdirectories, 1 level deep
    .AddWorkspaceChildType<BotEntity>(recursive: false)                      // Flat files only
```

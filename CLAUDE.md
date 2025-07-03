# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Important Note

**This repository contains framework libraries and abstractions.** 

**For primary development work, see: `/mnt/c/src/Trading.Proprietary/CLAUDE.md`**

The Trading.Proprietary repository contains the main trading applications, production deployments, and platform implementations that build upon the framework provided by this repository.

## Repository Role

This Trading repository provides:
- **Framework Libraries**: Core abstractions and interfaces
- **Automation Engine**: Bot execution and optimization framework  
- **Indicators System**: Technical analysis indicators
- **Historical Data**: Data management and backtesting infrastructure
- **Testing**: Unit and integration tests for the framework

## Build System

- **Primary solution**: `/mnt/c/src/Internal/LionFire.All.Trading.slnf` (cross-repository solution file)
- **SDK Version**: .NET 9.0 (fixed version, no rollForward)
- **Note**: Local Trading.sln has been removed as it was obsolete

### Building and Testing
```bash
# Build the cross-repository trading solution (recommended)
dotnet build /mnt/c/src/Internal/LionFire.All.Trading.slnf

# Run framework tests
dotnet test tests/

# Run specific test project
dotnet test tests/LionFire.Trading.Tests/
dotnet test tests/LionFire.Trading.Automation.Tests/
dotnet test tests/LionFire.Trading.Indicators.Tests/
```

## Development Workflow

**For most development work:**
1. See `/mnt/c/src/Trading.Proprietary/CLAUDE.md` for comprehensive guidance
2. Use `/mnt/c/src/Internal/LionFire.All.Trading.slnf` solution file
3. Framework changes happen here; application features happen in Trading.Proprietary

### Project Structure
- **SDK Version**: .NET 9.0 (fixed version, no rollForward)
- **Package Management**: Central package management via `Directory.Packages.props`
- **Output Paths**: Custom build paths to `z:\build\` (configured in Directory.Build.props)

## Architecture Overview

### Core Components

**Abstractions Layer (`LionFire.Trading.Abstractions`)**
- Defines fundamental interfaces and contracts for the entire trading system
- Contains data structures for bars, ticks, symbols, time frames
- Defines bot interfaces, account interfaces, and market data structures
- DataFlow system with inputs, outputs, parameters, and processors

**Automation System (`LionFire.Trading.Automation`)**
- Bot execution framework with harnesses for backtesting and live trading
- Optimization engine with grid search and parameter enumeration
- Backtesting engine with batch processing capabilities
- Multi-backtest context for portfolio optimization

**Historical Data System (`LionFire.Trading.HistoricalData`)**
- File-based storage and retrieval of market data
- Chunked data management for efficient storage
- Integration with external data providers
- Memory caching layer for performance

**Indicators System (`LionFire.Trading.Indicators`)**
- Wrapper around QuantConnect indicators
- Historical and real-time indicator execution harnesses
- Integration with market data feeds

### Key Frameworks and Technologies

**Orleans Integration**
- Grains for distributed processing (`LionFire.Trading.Grains.Abstractions`)
- Binance-specific grains for market data (`LionFire.Trading.Binance.Grains`)
- Broadcast channels for real-time data distribution

**Blazor Web UI**
- Multiple Blazor projects for different concerns (Analysis, Automation, Link)
- MudBlazor and ScottPlot for charting and visualization
- Real-time optimization monitoring and bot management

**External Integrations**
- Binance API integration for cryptocurrency trading
- cTrader platform integration
- QuantConnect indicators library
- QuickFix for FIX protocol trading

### Data Flow Architecture

The system uses a sophisticated DataFlow architecture:
- **Inputs**: `IPInput`, `IPKlineInput` for market data ingestion
- **Processors**: `IPMarketProcessor` for data transformation
- **Outputs**: Signal generation and trade execution
- **Parameters**: Hierarchical parameter system with optimization support
- **Slots**: Input/output connection system with type safety

### Bot Framework

**Bot Lifecycle**
- Bot2 base classes with standardized execution patterns
- Parameter injection via `PBot2` parameter objects
- Account simulation with `PBacktestAccount` and `PSimulatedAccount2`
- Position management with automatic risk controls

**Optimization System**
- Multi-dimensional parameter optimization
- Grid search with configurable levels of detail
- Batch backtesting with parallel execution
- Results analysis and ranking

### Data Storage

**Historical Data**
- File system based with efficient chunking
- Support for multiple exchanges and timeframes
- Automatic gap detection and filling
- Compression and archival capabilities

**Results Persistence**
- JSON-based backtest result storage
- CSV export for analysis
- Trade journal with detailed statistics
- Optimization run tracking and comparison

## Development Patterns

### Parameter System
Use the hierarchical parameter system for bot configuration:
- Mark parameters with `[Parameter]` attribute
- Use `IParametersFor<T>` interface for type safety
- Support optimization via `IParameterOptimizationOptions`

### Testing Framework
- Uses xUnit for unit testing
- Test projects follow naming convention: `*.Tests`
- Integration tests for backtesting and optimization workflows

### Dependency Injection
- Heavy use of Microsoft.Extensions.DependencyInjection
- Hosting extensions in each module (`*HostingX.cs` files)
- Service registration follows builder pattern

## Key External Dependencies

- **Orleans**: Distributed actor framework
- **MudBlazor**: Blazor component library
- **ScottPlot**: Charting and visualization
- **QuantConnect.Indicators**: Technical analysis indicators
- **Binance.Net**: Cryptocurrency exchange API
- **Serilog**: Structured logging
- **Marten**: PostgreSQL document database (where used)
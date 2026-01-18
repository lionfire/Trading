# Technology Stack

**Analysis Date:** 2026-01-18

## Languages

**Primary:**
- C# (latest language version) - All source code across ~60 projects

**Secondary:**
- HJSON - Configuration files
- JSON - API data, settings, serialization
- YAML - Some configuration (YamlDotNet package)

## Runtime

**Environment:**
- .NET 10.0 (target framework for all projects)

**SDK Version:**
- .NET SDK 10.0.100 (specified in `global.json`)
- Roll-forward policy: `latestMinor`

**Package Manager:**
- NuGet with Central Package Management
- Lockfile: Not used
- Version file: `Directory.Packages.props`

## Frameworks

**Core:**
- Microsoft.Extensions.Hosting 10.0.1 - Host application infrastructure
- Microsoft.Extensions.DependencyInjection 10.0.1 - Dependency injection
- Microsoft.Orleans.Sdk 9.1.2 - Distributed actor framework (grains)
- ReactiveUI 20.2.45 - Reactive MVVM framework
- System.Reactive 6.1.0 - Reactive extensions

**Web/UI:**
- ASP.NET Core 10.0 - Web framework (Blazor Server)
- MudBlazor 8.15.0 - Blazor component library
- Radzen.Blazor 6.1.6 - Alternative Blazor components
- ScottPlot 5.0.54 - Charting library
- Plotly.Blazor 6.0.2 - Interactive charts
- LightweightCharts.Blazor 5.0.8.2 - Financial charts

**Testing:**
- xUnit 2.9.3 - Test framework
- FluentAssertions 7.0.0 - Assertion library
- Moq 4.20.72 - Mocking framework
- coverlet.collector 6.0.4 - Code coverage
- BenchmarkDotNet 0.14.0 - Performance benchmarking

**Build/Dev:**
- Microsoft.NET.Test.Sdk 18.0.1 - Test SDK
- Microsoft.CodeAnalysis 4.14.0 - Roslyn for code generation

## Key Dependencies

**Critical (Trading-Specific):**
- QuantConnect.Indicators 2.5.15995 - Technical analysis indicators library
- Binance.Net 12.0.0 - Binance exchange API client
- Bybit.Net 3.14.2 - Bybit exchange API client
- cTrader.Automate 1.0.10 - cTrader platform integration
- QuickFix.Net.NETCore 1.8.1 - FIX protocol implementation

**Serialization:**
- System.Text.Json 10.0.1 - Primary JSON serialization
- Newtonsoft.Json 13.0.4 - Secondary JSON serialization
- MemoryPack 1.21.4 - Binary serialization
- Utf8Json 1.3.7 - High-performance JSON
- Alexinea.ZeroFormatter 1.6.4 - Zero-copy serialization
- CsvHelper 33.0.1 - CSV parsing/writing

**Compression:**
- K4os.Compression.LZ4.Streams 1.3.8 - LZ4 compression for historical data

**Infrastructure:**
- StackExchange.Redis 2.8.31 - Redis client
- Winton.Extensions.Configuration.Consul 3.4.0 - Consul configuration
- Polly 8.6.5 - Resilience and retry policies

**HTTP/Networking:**
- Flurl.Http 4.0.2 - Fluent HTTP client
- Microsoft.Extensions.Http 10.0.0 - HttpClientFactory

**Logging:**
- Serilog.Extensions.Hosting 9.0.0 - Structured logging
- NLog 5.4.0 - Alternative logging (legacy projects)
- OpenTelemetry packages (in Directory.Packages.props) - Observability

**CLI:**
- McMaster.Extensions.Hosting.CommandLine 4.1.1 - CLI hosting
- Spectre.Console 0.54.0 - Console UI
- Oakton 6.3.0 - Command-line parsing

**Reactive/Data:**
- DynamicData 9.1.2 - Reactive collections
- CircularBuffer 1.4.0 - Ring buffer implementation

## Configuration

**Environment:**
- Environment variables with `__` to `:` conversion for hierarchy
- `.env` file support (default: `/srv/trading/.env`)
- appsettings.json, appsettings.{Environment}.json, appsettings.local.json
- Command-line arguments

**Key Config Locations:**
- `src/LionFire.Trading.Hosting/Configuration/TradingConfigurationExtensions.cs` - Configuration loading
- `src/LionFire.Trading.Automation.Worker/appsettings.json` - Orleans and logging config
- `src/LionFire.Trading.Cli/appsettings.json` - CLI tool config

**Build:**
- `Directory.Build.props` - Global build properties
- `Directory.Packages.props` - Central package versions
- `global.json` - SDK version pinning
- Custom output paths to `z:\build\` (Windows build system)

## Platform Requirements

**Development:**
- .NET SDK 10.0.100+
- Windows (primary) or Linux (WSL2 supported)
- Visual Studio 2022 or Rider recommended
- Build solution: `/mnt/c/src/Internal/LionFire.All.Trading.slnf`

**Production:**
- .NET 10.0 runtime
- Linux servers (Ubuntu/Debian typical)
- Consul for Orleans clustering (optional)
- Redis for caching/pub-sub (optional)

**Package Output:**
- NuGet packages auto-pushed to `LionFireLocal` feed on Release builds
- Symbol packages (.snupkg) included
- Version: 7.0.0-alpha (from Directory.Build.props)

---

*Stack analysis: 2026-01-18*

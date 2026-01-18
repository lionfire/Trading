# External Integrations

**Analysis Date:** 2026-01-18

## APIs & External Services

**Cryptocurrency Exchanges:**

- **Binance** - Primary crypto exchange integration
  - SDK: `Binance.Net` 12.0.0
  - Auth: `Binance:ApiKey`, `Binance:ApiSecret` (env vars)
  - Base URL: `https://api.binance.com`
  - WebSocket: `wss://stream.binance.com:9443`
  - Testnet: `https://testnet.binance.vision`, `wss://testnet.binance.vision`
  - Projects: `src/LionFire.Trading.Feeds.Binance/`, `src/LionFire.Trading.Binance.Grains/`
  - Capabilities: Market data, order execution, account management, USD futures

- **Bybit** - Secondary crypto exchange
  - SDK: `Bybit.Net` 3.14.2
  - Auth: `Bybit:ApiKey`, `Bybit:ApiSecret` (env vars)
  - Base URL: `https://api.bybit.com`
  - WebSocket: `wss://stream.bybit.com`
  - Project: `src/LionFire.Trading.Feeds.Bybit/`

- **MEXC** - Crypto exchange
  - SDK: Custom implementation
  - Auth: `Mexc:ApiKey`, `Mexc:ApiSecret` (env vars)
  - Base URL: `https://api.mexc.com`
  - WebSocket: `wss://wbs.mexc.com`
  - Project: `src/LionFire.Trading.Feeds.MEXC/`

- **Phemex** - Crypto derivatives exchange
  - SDK: Custom HTTP/WebSocket clients
  - Auth: `Phemex:ApiKey`, `Phemex:ApiSecret` (env vars)
  - Client: `src/LionFire.Trading.Phemex/Api/PhemexRestClient.cs`
  - WebSocket: `src/LionFire.Trading.Phemex/Api/PhemexWebSocketClient.cs`
  - Project: `src/LionFire.Trading.Phemex/`

**Forex/Traditional Trading:**

- **cTrader (Spotware)** - Forex trading platform
  - SDK: `cTrader.Automate` 1.0.10, custom OpenAPI libraries
  - Auth: OAuth2 (stored credentials)
  - Protocol: Custom binary protocol, REST API
  - Projects: `src/Spotware/LionFire.Trading.Spotware.Connect/`, `src/LionFire.Trading.cTrader/`
  - Features: Account management, trading API, historical data

- **TrueFX** - Forex data feed
  - SDK: Custom implementation
  - Project: `src/LionFire.Trading.Feeds.TrueFx/`

- **nj4x** - MetaTrader 4 bridge
  - SDK: `nj4x` 2.9.3
  - Project: `src/LionFire.Trading.nj4x/`

**Trading Protocols:**

- **FIX Protocol** - Financial Information eXchange
  - SDK: `QuickFix.Net.NETCore` 1.8.1
  - Spec: FIX 4.4
  - Project: `src/LionFire.Trading.QuickFix.ConsoleTest/`
  - Config: `spec/fix/FIX44.xml`, `config.demo.trade.ini`

## Data Storage

**Databases:**
- PostgreSQL (via Marten, Npgsql) - Available in packages but not actively used in this repo
- LiteDB 5.0.21 - Embedded document database (available)
- SQLite (via EF Core) - Available for local storage

**File Storage:**
- Local filesystem - Primary historical data storage
- Path: Configurable via `Windows:HistoricalData:BaseDir` (default: `F:\st\Investing-HistoricalData`)
- Format: LZ4-compressed binary files, CSV export support

**Caching:**
- Redis (StackExchange.Redis 2.8.31)
  - Used for: cTrader data caching, pub/sub
  - Project: `src/LionFire.Trading.cTrader.Redis/`
  - Project: `src/LionFire.Trading.Phemex/` (optional)

## Distributed Computing

**Orleans (Microsoft.Orleans.Sdk 9.1.2):**
- Purpose: Distributed actor model for bot execution and data streaming
- Clustering: Consul-based (`Microsoft.Orleans.Clustering.Consul`)
- Persistence: Redis available (`Microsoft.Orleans.Persistence.Redis`)
- Streaming: `Microsoft.Orleans.Streaming`, `Microsoft.Orleans.BroadcastChannel`
- Dashboard: `OrleansDashboard` 8.2.0
- Config: `Orleans:Enable`, `Orleans:Cluster:Kind`, `Orleans:Cluster:ClusterId`

**Grain Types:**
- `src/LionFire.Trading.Grains.Abstractions/` - Grain interfaces
- `src/LionFire.Trading.Binance.Grains/` - Binance-specific grains
- `src/LionFire.Trading.Automation.Orleans/` - Automation grains

## Service Discovery

**Consul:**
- Purpose: Orleans cluster membership, configuration
- SDK: `Consul` 1.7.14.2, `Winton.Extensions.Configuration.Consul` 3.4.0
- Config: `Orleans:Cluster:Kind: "Consul"`, `Orleans:Cluster:ClusterId`
- Project: `src/LionFire.Trading.Cli/` uses Consul configuration

## Authentication & Identity

**Exchange Auth:**
- API Key/Secret pattern for all exchanges
- Credentials stored in:
  - Environment variables
  - `.env` file (`/srv/trading/.env`)
  - appsettings.json (development only)

**cTrader OAuth:**
- OAuth2 flow for Spotware/cTrader
- Implementation: `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/ConnectOAuth.cs`

**Application Auth:**
- OpenIddict packages available in Directory.Packages.props (not actively used in this repo)
- JWT tokens for API authentication (configured in Trading.Proprietary)

## Monitoring & Observability

**Logging:**
- Primary: Serilog (structured logging)
  - Console sink
  - File sink with rotation
  - Configuration-based setup
- Secondary: NLog (legacy projects)
- Config section: `Serilog:MinimumLevel`

**Telemetry:**
- OpenTelemetry packages available:
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.11.2
  - `OpenTelemetry.Exporter.Console` 1.11.2
  - `OpenTelemetry.Exporter.Prometheus.AspNetCore` 1.10.0-beta.1
  - `OpenTelemetry.Instrumentation.AspNetCore` 1.11.1
- OTLP endpoint: `OTEL_EXPORTER_OTLP_ENDPOINT` (default: `http://localhost:4317`)

**Health Checks:**
- AspNetCore.HealthChecks packages available:
  - UI, Consul, Network, Npgsql, Redis, System

## CI/CD & Deployment

**Hosting:**
- Blazor Server applications
- Orleans silo hosting
- Docker support available (`Microsoft.VisualStudio.Azure.Containers.Tools.Targets`)

**Build:**
- Custom build paths: `z:\build\bin\`, `z:\build\obj\`
- NuGet package output on Release builds
- Local feed push: `LionFireLocal`

## Environment Configuration

**Required env vars for exchanges:**
```
Binance__ApiKey=...
Binance__ApiSecret=...
Bybit__ApiKey=...
Bybit__ApiSecret=...
Mexc__ApiKey=...
Mexc__ApiSecret=...
Phemex__ApiKey=...
Phemex__ApiSecret=...
```

**Orleans configuration:**
```
Orleans__Enable=true
Orleans__Cluster__Kind=Consul
Orleans__Cluster__ClusterId=blue
Orleans__Cluster__BaseServiceId=LionFire.Trading
```

**Secrets location:**
- `.env` file: `/srv/trading/.env` (Linux production)
- Environment variables (deployment)
- User secrets (development): `UserSecretsId` in .csproj files

## WebSockets & Real-time Data

**Incoming streams:**
- Exchange market data WebSocket feeds (Binance, Bybit, MEXC, Phemex)
- Real-time price updates
- Order book streaming
- Trade execution notifications

**Interface:**
- `src/LionFire.Trading.Grains.Abstractions/Streaming/IWebSocketManager.cs`
- `src/LionFire.Trading.Grains.Abstractions/Streaming/IWebSocketManagerMetrics.cs`

**Outgoing (Blazor):**
- Blazor Server SignalR connections
- Real-time UI updates via Orleans streaming

## Technical Analysis

**QuantConnect Indicators:**
- SDK: `QuantConnect.Indicators` 2.5.15995
- Wrapper: `src/LionFire.Trading.Indicators.QuantConnect/`
- Purpose: EMA, SMA, RSI, MACD, Bollinger Bands, ATR, etc.

---

*Integration audit: 2026-01-18*

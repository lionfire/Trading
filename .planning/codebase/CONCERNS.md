# Codebase Concerns

**Analysis Date:** 2026-01-18

## Tech Debt

**Legacy `_Old` Suffix Files:**
- Issue: Six legacy files with `_Old` suffix remain in codebase, indicating incomplete migrations
- Files:
  - `src/LionFire.Trading/Accounts/AccountBase_Old.cs`
  - `src/LionFire.Trading/Accounts/IAccount_Old.cs`
  - `src/LionFire.Trading/Accounts/Simulated/ISimulatedAccount_Old.cs`
  - `src/LionFire.Trading/Accounts/Simulated/SimulatedAccountBase_Old.cs`
  - `src/LionFire.Trading/Feeds/IFeed_Old.cs`
  - `src/LionFire.Trading/Indicators/Implementations/MovingAverages/SimpleMovingAverage_Old.cs`
- Impact: Confusion about which interfaces/classes to use; potential for both old and new code paths being exercised
- Fix approach: Audit usages of old classes, migrate remaining references, then remove old files

**Blocking Async Calls (sync-over-async anti-pattern):**
- Issue: Multiple instances of `.Wait()`, `.Result`, and `.GetAwaiter().GetResult()` blocking async operations
- Files:
  - `src/LionFire.Trading/Workspaces/Sessions/Session.cs:299` - `StartAsync().Wait(); // FIXME Async to sync`
  - `src/LionFire.Trading/Workspaces/TradingWorkspace.cs:337` - `session.Initialize().Wait()`
  - `src/LionFire.Trading.Shared/Bots/BotBase.cs:154` - `.GetAwaiter().GetResult()` in Mode setter
  - `src/LionFire.Trading.Shared/Indicators/IndicatorBase.cs:320` - `CalculateIndex(index).Wait()`
  - `src/LionFire.Trading.Automation/Backtesting2/InputEnumerators/AsyncInputEnumerator.cs:71` - `LoadMore().Wait()`
  - `src/LionFire.Trading.Phemex/WebSocket/PhemexWebSocketClient.cs:255` - `DisconnectAsync().GetAwaiter().GetResult()`
  - `src/LionFire.Trading/Markets/Symbol.cs:212` - `.Wait()` call
  - `src/LionFire.Trading/Indicators/IndicatorProvider.cs:42` - `.Result` property access
- Impact: Thread pool starvation, deadlocks in certain contexts, poor scalability
- Fix approach: Refactor to use async/await throughout, or use `ConfigureAwait(false)` where blocking is unavoidable

**`async void` Methods:**
- Issue: Fire-and-forget async methods that swallow exceptions and cannot be awaited
- Files:
  - `src/LionFire.Trading.Automation.Blazor/Optimization/OneShotOptimizeVM.cs:556` - `async void OnExportToBot`
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.cs:250` - `async void OnTradeApiEnabledChanging`
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.cs:738` - `async void Series_BarHasObserversChanged`
  - `src/LionFire.Trading.Shared/Bots/BotBase.cs:280` - `async void OnLinkStatusTimerElapsed`
  - `src/LionFire.Trading/Indicators/SingleSeriesIndicatorBase.LionFire.cs:48` - `async void barHandler`
- Impact: Unhandled exceptions crash process; cannot propagate errors to callers
- Fix approach: Convert to `async Task` and handle at call sites, or add explicit try-catch with logging

**Hardcoded Paths:**
- Issue: File system paths hardcoded directly in source code
- Files:
  - `src/LionFire.Trading.QuickFix.ConsoleTest/MyQuickFixApp.cs:114` - `@"C:\ProgramData\Trading"` HARDPATH
  - `src/LionFire.Trading.Cli/Program.cs:33-34` - Historical data base dirs HARDCODE
  - `src/LionFire.Trading/Backtesting/Injesting/IngestOptions.cs:12-17` - Multiple F: drive paths HARDCODE HARDPATH
  - `src/LionFire.Trading/Data/HistoricalDataCacheFile.cs:75` - `@"d:\st\Investing-Data\MarketData"` HARDPATH
  - `src/LionFire.Trading/Data/DataSourceCollection.cs:58` - `@"c:\TickDownloader\tickdata\"` HARDCODE HARDPATH
- Impact: Breaks deployments to different environments; violates 12-factor app principles
- Fix approach: Move to configuration options with sensible defaults; use environment variables

**Incomplete Feed Implementations:**
- Issue: Feed collector implementations are stubs or throw NotImplementedException
- Files:
  - `src/LionFire.Trading.Feeds.MEXC/MexcFeedCollector.cs:36-46` - Entire implementation is placeholder
  - `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:84` - `throw new NotImplementedException("Binance feed subscription needs to be updated")`
  - `src/LionFire.Trading.Feeds.Simulated/SimulatedFeed.cs:144-164` - Multiple `throw new NotImplementedException()` properties
- Impact: MEXC exchange not usable; Binance feed collection broken; simulated feeds incomplete
- Fix approach: Implement MEXC client or remove project; update Binance.Net API usage; complete SimulatedFeed

**NotImplementedException in Production Code:**
- Issue: NotImplementedException thrown in multiple locations indicating incomplete features
- Files:
  - `src/LionFire.Trading.HistoricalData/Data/Bars/Commands/RetrieveHistoricalDataJob.cs:624,663,682,774` - TimeFrame handling
  - `src/LionFire.Trading.Exchanges/Services/ExchangeClientFactory.cs:37,52` - MEXC client not implemented
  - `src/LionFire.Trading.Feeds.TrueFx/TrueFxFeed.cs:90-92` - Server time properties
  - `tests/LionFire.Trading.Tests/Hosting/ServiceProviderProvider.cs:39` - `.ConfigureBacktestingOptions`
- Impact: Runtime failures when code paths are hit
- Fix approach: Implement missing functionality or throw more descriptive exceptions explaining limitations

## Known Bugs

**FIXME Comments in Code:**
- Symptoms: Code explicitly marked as needing fixes
- Files:
  - `src/LionFire.Trading/Workspaces/Sessions/Session.cs:299` - `// FIXME Async to sync`
  - `src/LionFire.Trading.QuickFix.ConsoleTest/MyQuickFixApp.cs:74` - FIXME on header field access
- Trigger: Running affected code paths
- Workaround: None documented

**TODO Items Indicating Missing Functionality:**
- Symptoms: Features marked as incomplete
- Files:
  - `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:51` - API credentials configuration
  - `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:185-186` - Bid/ask prices using trade price
  - `src/LionFire.Trading.Feeds.Bybit/BybitFeedCollector.cs:192-193` - Same bid/ask issue
  - `src/LionFire.Trading.Feeds.Simulated/SimulatedFeed.cs:79` - tickTimingVariancePercent
- Trigger: Using affected exchanges for trading with bid/ask spreads
- Workaround: Trade prices used instead of actual bid/ask

## Security Considerations

**API Credentials in Configuration:**
- Risk: API keys and secrets passed via command-line flags or environment variables could be logged
- Files:
  - `src/LionFire.Trading.Cli/Phemex/PhemexCommands.cs:34-38` - API key/secret as CLI flags
  - `src/LionFire.Trading.Feeds.MEXC/MexcFeedCollector.cs:15-16` - ApiKey/ApiSecret in options
  - `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:25-26` - ApiKey/ApiSecret in options
- Current mitigation: Environment variables supported
- Recommendations: Use secure secret stores (Azure Key Vault, AWS Secrets Manager); avoid CLI flag credentials

**Broad Exception Handling:**
- Risk: Catching generic `Exception` can mask security-relevant errors
- Files: 50+ locations catching `catch (Exception ex)` throughout:
  - `src/LionFire.Trading.Feeds.Bybit/BybitFeedCollector.cs` - multiple locations
  - `src/LionFire.Trading.Automation.Orleans/OptimizationQueueProcessor.cs` - multiple locations
  - `src/LionFire.Trading.Shared/Indicators/IndicatorBase.cs:165,173,212,254`
- Current mitigation: Exceptions are logged
- Recommendations: Catch specific exception types; review for exception swallowing

## Performance Bottlenecks

**No Memory-Efficient Data Structures:**
- Problem: Limited use of `Span<T>`, `Memory<T>`, or `ArrayPool<T>` for large data processing
- Files: Only 1 file uses these constructs:
  - `src/LionFire.Trading.HistoricalData/Data/Bars/Service/CompositeHistoricalDataProvider2.cs`
- Cause: Historical data operations copy arrays frequently
- Improvement path: Use `Span<T>` for slicing; `ArrayPool<T>` for temporary buffers; `Memory<T>` for async operations

**Large Monolithic Classes:**
- Problem: Several files exceed 800+ lines indicating potential complexity
- Files:
  - `src/LionFire.Trading.Shared/Bots/BotBase.cs` - 1797 lines
  - `src/LionFire.Trading.Cli/Commands/PhemexHandlers.cs` - 1813 lines
  - `src/LionFire.Trading.Cli/Phemex/PhemexCommands.cs` - 1103 lines
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.TradeApi.cs` - 1118 lines
  - `src/LionFire.Trading.Automation/Backtesting2/Execution/BatchHarness.cs` - 997 lines
  - `src/LionFire.Trading.Automation.Blazor/Optimization/OneShotOptimizeVM.cs` - 965 lines
- Cause: Accumulated functionality over time without refactoring
- Improvement path: Extract related methods into focused classes; apply single responsibility principle

**Generated Protobuf Files:**
- Problem: Very large auto-generated protobuf message files
- Files:
  - `src/Spotware/OpenApiCSharpNewLibrary/OpenApiMessages.cs` - 9791 lines
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Protocol/OpenApiCSharpProtoNewLibrary/OpenApiMessages.cs` - 10147 lines
  - `src/Spotware/OpenApiCSharpNewLibrary/OpenApiModelMessages.cs` - 3764 lines
- Cause: Auto-generated code from cTrader OpenAPI
- Improvement path: These are generated; consider if all message types are needed

## Fragile Areas

**cTrader Account Integration:**
- Files:
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.cs`
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.TradeApi.cs`
- Why fragile: `#pragma warning disable CS4014` (unawaited async calls); async void methods; complex state management
- Safe modification: Add comprehensive logging before changes; test with demo account
- Test coverage: Limited

**Session and Workspace Management:**
- Files:
  - `src/LionFire.Trading/Workspaces/Sessions/Session.cs` - 676 lines
  - `src/LionFire.Trading/Workspaces/TradingWorkspace.cs`
- Why fragile: Sync-over-async patterns; complex initialization state machine
- Safe modification: Trace state transitions; ensure proper cleanup
- Test coverage: Minimal

**Indicator System:**
- Files:
  - `src/LionFire.Trading.Shared/Indicators/IndicatorBase.cs`
  - `src/LionFire.Trading/Indicators/SingleSeriesIndicatorBase.LionFire.cs`
- Why fragile: Multiple catch blocks that rethrow with wrapping; conditional compilation (`#if cAlgo`)
- Safe modification: Unit test indicator calculations; verify against known values
- Test coverage: Good for individual indicators (384 tests), but base class coverage unclear

## Scaling Limits

**Synchronous Data Operations:**
- Current capacity: Single-threaded historical data retrieval
- Limit: Large backtests with many symbols blocked on sequential data loading
- Scaling path: Implement parallel data fetching; use async enumerable patterns

**Optimization Queue Processing:**
- Current capacity: `OptimizationQueueProcessor` uses semaphores for concurrency control
- Limit: Configured by semaphore count; Orleans grain per-queue
- Scaling path: Distributed Orleans cluster; horizontal scaling of worker nodes

## Dependencies at Risk

**Binance.Net API Compatibility:**
- Risk: Current implementation explicitly states it needs updating for current Binance.Net version
- Impact: Feed collection broken
- Migration plan: Update to Binance.Net 12.0.0 API; see `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:82`

**Obsolete Internal APIs:**
- Risk: Multiple files marked with `[Obsolete]` attribute still in use
- Impact: Potential breaking changes when removed
- Migration plan: Audit usage of:
  - `src/LionFire.Trading.HistoricalData/Data/HistoricalDataProvider2Base.cs` (obsolete class)
  - `src/LionFire.Trading.HistoricalData/Data/Bars/_Obsolete/IHistoricalDataProvider2.cs`
  - `src/LionFire.Trading/Accounts/AccountParticipant.cs` (obsolete class)

## Missing Critical Features

**MEXC Exchange Integration:**
- Problem: MEXC feed collector is a placeholder with no implementation
- Blocks: Cannot use MEXC exchange for data collection or trading
- Files: `src/LionFire.Trading.Feeds.MEXC/MexcFeedCollector.cs`

**Accurate Bid/Ask Spreads:**
- Problem: Feed collectors use trade price for both bid and ask
- Blocks: Realistic slippage simulation in backtests
- Files:
  - `src/LionFire.Trading.Feeds.Binance/BinanceFeedCollector.cs:185-186`
  - `src/LionFire.Trading.Feeds.Bybit/BybitFeedCollector.cs:192-193`

## Test Coverage Gaps

**Overall Test Coverage:**
- What's not tested: ~164,000 lines of source code vs ~18,000 lines of test code (11% by lines)
- Files: 73 test files covering 1,225 source files
- Risk: Large surface area without automated verification
- Priority: High

**Specific Untested Areas:**
- What's not tested: Live trading account integrations
- Files:
  - `src/Spotware/LionFire.Trading.Spotware.Connect/Accounts/CTraderAccount.cs`
  - `src/LionFire.Trading.Phemex/` - only 4 tests skipped requiring live connection
- Risk: Trading logic failures only discovered in production
- Priority: High

**Bot Framework Tests:**
- What's not tested: BotBase and bot execution lifecycle
- Files:
  - `src/LionFire.Trading.Shared/Bots/BotBase.cs` - 1797 lines, minimal test coverage
  - `src/LionFire.Trading.Automation/Automation/Bots/` - bot execution infrastructure
- Risk: Bot state management bugs; position calculation errors
- Priority: High

**Session/Workspace Tests:**
- What's not tested: Trading session lifecycle, workspace initialization
- Files:
  - `src/LionFire.Trading/Workspaces/Sessions/Session.cs`
  - `src/LionFire.Trading/Workspaces/TradingWorkspace.cs`
- Risk: Resource leaks; initialization failures
- Priority: Medium

---

*Concerns audit: 2026-01-18*

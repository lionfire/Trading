# Testing Patterns

**Analysis Date:** 2026-01-18

## Test Framework

**Runner:**
- xUnit (version 2.9.3)
- Config: Central package management via `Directory.Packages.props`

**Assertion Libraries:**
- xUnit native `Assert` class (primary)
- FluentAssertions (version 7.0.0) - for fluent syntax

**Mocking:**
- Moq (version 4.20.72)

**Run Commands:**
```bash
dotnet test tests/                                    # Run all tests
dotnet test tests/LionFire.Trading.Tests/             # Run specific project
dotnet test tests/ --filter "FullyQualifiedName~ATR"  # Filter tests
dotnet test tests/ --collect:"XPlat Code Coverage"    # With coverage
```

## Test File Organization

**Location:**
- Separate `tests/` directory at repository root
- Test projects mirror source project naming with `.Tests` suffix

**Structure:**
```
tests/
├── LionFire.Trading.Tests/                    # Core trading tests
│   ├── Hosting/
│   │   └── ServiceProviderProvider.cs         # DI test infrastructure
│   ├── Indicators/
│   │   └── StochasticTests.cs
│   └── Tests/
│       └── BinanceDataTest.cs                 # Base class for data tests
├── LionFire.Trading.Automation.Tests/         # Automation tests
│   ├── Backtesting/
│   │   └── Backtest_.cs
│   ├── FillSimulation/
│   │   └── SimpleFillSimulatorTests.cs
│   ├── PriceMonitoring/
│   │   └── PendingOrderTests.cs
│   └── Queue/
│       ├── Infrastructure/
│       │   └── OrleansTestCluster.cs          # Orleans test setup
│       ├── Integration/
│       │   └── OptimizationQueueIntegrationTests.cs
│       └── Unit/
│           └── OptimizationQueueGrainTests.cs
├── LionFire.Trading.Indicators.Tests/         # Indicator tests
│   ├── EMATests.cs
│   ├── SMATests.cs
│   ├── ATRTests.cs
│   └── Indicators/                            # Organized by indicator
│       └── QuantConnect/
│           └── AverageTrueRange/
└── LionFire.Trading.Phemex.Tests/             # Exchange integration tests
    ├── Mocks/
    │   └── MockPhemexClients.cs
    ├── Rest/
    │   └── PhemexRestClientTests.cs
    └── WebSocket/
        └── PhemexWebSocketClientTests.cs
```

**Naming:**
- Test files: `{Feature}Tests.cs` (e.g., `EMATests.cs`, `SimpleFillSimulatorTests.cs`)
- Test classes: `{Feature}Tests` (e.g., `EMATests`, `ATRTests`)
- Test methods: `{Method}_{Scenario}_{ExpectedResult}` or descriptive name

## Test Structure

**Suite Organization:**
```csharp
public class SimpleFillSimulatorTests
{
    private readonly SimpleFillSimulator<decimal> _simulator = new();

    #region Market Order Tests

    [Fact]
    public void MarketOrder_Long_FillsAtAsk()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.05m);
    }

    #endregion
}
```

**Patterns:**
- **Arrange-Act-Assert (AAA):** Standard test structure
- **Regions:** Group related tests with `#region` blocks
- **Field initialization:** Initialize shared fixtures as class fields
- **Comments in assertions:** Document expected values

## Global Usings for Tests

**Location:** `GlobalUsings.cs` in each test project

**Example from `tests/LionFire.Trading.Indicators.Tests/GlobalUsings.cs`:**
```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using LionFire.Trading;
global using LionFire.Structures;
global using LionFire.Trading.Indicators;
global using LionFire.Trading.Indicators.Harnesses;
global using Xunit;
global using Microsoft.Extensions.DependencyInjection;
```

**Example from `tests/LionFire.Trading.Automation.Tests/GlobalUsings.cs`:**
```csharp
global using LionFire.Trading;
global using System;
global using Xunit;
global using System.Collections.Generic;
global using FluentAssertions;
global using Moq;
global using Orleans;
global using LionFire.Trading.Optimization.Queue;
global using LionFire.Trading.Grains.Optimization;
```

## Mocking

**Framework:** Moq

**Patterns - Manual Mocks:**
```csharp
// From tests/LionFire.Trading.Phemex.Tests/Mocks/MockPhemexClients.cs
public class MockPhemexWebSocketClient : IPhemexWebSocketClient
{
    private readonly Subject<string> _messageSubject = new();
    private bool _isConnected;

    public IObservable<string> Messages => _messageSubject.AsObservable();
    public bool IsConnected => _isConnected;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    public void SendMockTick(string symbol, decimal price, decimal volume, string side = "Buy")
    {
        // Emit mock data
        _messageSubject.OnNext(JsonConvert.SerializeObject(tick));
    }
}
```

**What to Mock:**
- External services (WebSocket clients, REST clients)
- Time-dependent components
- File system access for isolated tests
- Orleans grains (use `Microsoft.Orleans.TestingHost`)

**What NOT to Mock:**
- Core algorithm logic (test directly)
- Data structures and models
- Simple value calculations

## Orleans Test Infrastructure

**Location:** `tests/LionFire.Trading.Automation.Tests/Queue/Infrastructure/OrleansTestCluster.cs`

**Pattern:**
```csharp
public class OrleansTestCluster : IDisposable
{
    private TestCluster? _cluster;

    public async Task StartAsync()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        builder.AddClientBuilderConfigurator<TestClientConfigurator>();
        _cluster = builder.Build();
        await _cluster.DeployAsync();
    }

    public IOptimizationQueueGrain GetQueueGrain(string grainId = "test-queue")
    {
        return GrainFactory.GetGrain<IOptimizationQueueGrain>(grainId);
    }
}

public class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder
            .UseInMemoryReminderService()
            .AddMemoryGrainStorageAsDefault()
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
    }
}
```

## Service Provider for Tests

**Location:** `tests/LionFire.Trading.Tests/Hosting/ServiceProviderProvider.cs`

**Pattern:**
```csharp
public static class ServiceProviderProvider
{
    public static IServiceProvider ServiceProvider => serviceProvider ??= Init();

    private static IServiceProvider Init()
    {
        var Configuration = new ConfigurationManager();
        Configuration.AddInMemoryCollection([
            new ("LionFire:Trading:HistoricalData:Windows:BaseDir", @"F:\st\Investing-HistoricalData\"),
        ]);
        Configuration.AddEnvironmentVariables("DOTNET__");

        IServiceCollection services = new ServiceCollection();
        services
            .AddOptions()
            .AddLogging(b => b.AddConsole())
            .AddSingleton<IConfiguration>(Configuration)
            .AddHistoricalBars(Configuration)
            .AddIndicators();

        return services.BuildServiceProvider();
    }
}
```

## Test Base Classes

**Location:** `tests/LionFire.Trading.Tests/Tests/BinanceDataTest.cs`

**Pattern:**
```csharp
public class BinanceDataTest
{
    public IServiceProvider ServiceProvider => ServiceProviderProvider.ServiceProvider;

    public DateChunker HistoricalDataChunkRangeProvider =>
        ServiceProvider.GetRequiredService<DateChunker>();

    public IHistoricalTimeSeries<T> Resolve<T>(object source) =>
        ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve<T>(source);
}
```

## Async Testing

**Pattern:**
```csharp
public class OptimizationQueueGrainTests : IAsyncLifetime
{
    private OrleansTestCluster _cluster = null!;
    private IOptimizationQueueGrain _queueGrain = null!;

    public async Task InitializeAsync()
    {
        _cluster = new OrleansTestCluster();
        await _cluster.StartAsync();
        _queueGrain = _cluster.GetQueueGrain("test-queue");
    }

    public async Task DisposeAsync()
    {
        await _cluster.StopAsync();
        _cluster.Dispose();
    }

    [Fact]
    public async Task EnqueueJobAsync_ShouldCreateJobWithCorrectProperties()
    {
        var job = await _queueGrain.EnqueueJobAsync(parametersJson, priority, submittedBy);
        job.Should().NotBeNull();
        job.Status.Should().Be(OptimizationJobStatus.Queued);
    }
}
```

## Theory Tests

**Pattern:**
```csharp
[Theory]
[InlineData(1)]
[InlineData(5)]
[InlineData(20)]
[InlineData(50)]
[InlineData(200)]
public void DifferentPeriods_CalculateCorrectly(int period)
{
    var parameters = new PSMA<double, double> { Period = period };
    var sma = new SMA_FP<double, double>(parameters);
    var inputs = Enumerable.Repeat(10.0, period * 2).ToArray();
    var outputs = new double[inputs.Length];

    sma.OnBarBatch(inputs, outputs);

    for (int i = period - 1; i < outputs.Length; i++)
    {
        Assert.Equal(10.0, outputs[i]);
    }
}

[Theory]
[InlineData(FillOrderType.Market)]
[InlineData(FillOrderType.Limit)]
[InlineData(FillOrderType.Stop)]
public void AllOrderTypes_HaveZeroSlippage(FillOrderType orderType)
{
    // Test across multiple enum values
}
```

## Indicator Test Patterns

**Standard structure:**
```csharp
public class EMATests
{
    [Fact]
    public void EMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 3 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = new double[] { 2, 4, 6, 8, 12, 14, 16, 18, 20 };
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);
        Assert.True(double.IsNaN(outputs[0])); // Not ready
        Assert.Equal(4, outputs[2], 2);        // First valid value
    }

    [Fact]
    public void EMA_Clear_ResetsState()
    {
        // Test that Clear() properly resets indicator state
    }

    [Fact]
    public void EMA_HandlesVolatileData()
    {
        // Test with volatile/edge case data
    }
}
```

## Coverage

**Requirements:** No enforced minimums

**Coverage Collection:**
- Package: `coverlet.collector` (version 6.0.4)

**View Coverage:**
```bash
dotnet test tests/ --collect:"XPlat Code Coverage"
```

## Test Types

**Unit Tests:**
- Test individual classes/methods in isolation
- Mock external dependencies
- Fast execution
- Located in `Unit/` subdirectories

**Integration Tests:**
- Test multiple components together
- Use real Orleans test cluster
- May require external resources
- Located in `Integration/` subdirectories

**E2E Tests:**
- Not widely used in this codebase
- Some integration tests with data cover end-to-end scenarios

## Common Patterns

**FluentAssertions:**
```csharp
result.IsFilled.Should().BeTrue();
result.ExecutionPrice.Should().Be(100.05m);
result.FilledQuantity.Should().Be(1.0m);
result.Reason.Should().Contain("Stop not triggered");
job.CreatedTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
```

**xUnit Assert:**
```csharp
Assert.True(sma.IsReady);
Assert.Equal(4, outputs[2], 2);  // precision parameter
Assert.True(double.IsNaN(outputs[0]));
Assert.ThrowsAsync<ArgumentException>(async () => { ... });
```

**Error Testing:**
```csharp
[Fact]
public void LimitOrder_NoLimitPrice_Fails()
{
    var request = new FillRequest<decimal>
    {
        OrderType = FillOrderType.Limit,
        LimitPrice = null
    };

    var result = _simulator.CalculateFill(request);

    result.IsFilled.Should().BeFalse();
    result.Reason.Should().Contain("Limit price not specified");
}
```

## Excluded/Broken Tests

**Pattern from csproj:**
```xml
<!-- Exclude broken tests that have pre-existing issues -->
<ItemGroup>
    <Compile Remove="Queue\**\*.cs" />
</ItemGroup>
```

---

*Testing analysis: 2026-01-18*

# Coding Conventions

**Analysis Date:** 2026-01-18

## Naming Patterns

**Files:**
- Classes: PascalCase matching class name (e.g., `AtrBot.cs`, `BacktestQueue.cs`)
- Interfaces: `I` prefix + PascalCase (e.g., `IAccount2.cs`, `IIndicatorHarness.cs`)
- Parameter classes: `P` prefix + Type name (e.g., `PEMA.cs`, `PAtrBot.cs`, `PPointsBot.cs`)
- Template/Configuration classes: `T` prefix + Type name (e.g., `TBot.cs`)
- Extension methods: Suffix with `X` (e.g., `IndicatorsHostingX.cs`, `BacktestingHostingX.cs`)
- Blazor components: PascalCase (e.g., `OneShotOptimize.razor`, `TradingChart.razor`)

**Classes:**
- Standard classes: PascalCase (e.g., `BacktestQueue`, `MultiBacktestContext`)
- Bot classes: Suffix with `Bot` (e.g., `AtrBot`, `ChandelierExitBot`)
- Parameter classes: Prefix with `P` (e.g., `PAtrBot<TValue>`, `PEMA<TPrice, TOutput>`)
- Grain interfaces: Prefix with `I`, suffix with `G` (e.g., `IOptimizationQueueGrain`, `IUserPreferencesG`)
- Hosting extensions: Suffix with `HostingX` (e.g., `BacktestingHostingX`, `IndicatorsHostingX`)

**Functions:**
- Public methods: PascalCase (e.g., `OnBar()`, `CalculateFill()`, `EnqueueJobAsync()`)
- Async methods: Suffix with `Async` (e.g., `StartAsync()`, `DequeueJobAsync()`)
- Event handlers: Prefix with `On` (e.g., `OnBar()`, `OnBarBatch()`)
- Factory methods: Prefix with `Create` (e.g., `Create()`)

**Variables:**
- Private fields: camelCase with underscore prefix for some, no prefix for others (mixed convention)
  - `_cluster`, `_simulator` in test files
  - `buffer`, `sum`, `barIndex` in implementation files
- Local variables: camelCase (e.g., `outputs`, `parameters`, `request`)
- Parameters: camelCase (e.g., `serviceProvider`, `exchangeSymbolTimeFrame`)

**Types:**
- Generic type parameters: Single letter or descriptive (e.g., `TValue`, `TInput`, `TOutput`, `TPrecision`)
- Enums: PascalCase (e.g., `FillOrderType`, `OptimizationJobStatus`, `LongAndShort`)

## Code Organization

**Region Usage:**
- Use `#region` / `#endregion` blocks to organize code sections
- Common regions: `Static`, `Parameters`, `Lifecycle`, `State`, `Event Handling`, `Methods`, `Validation`
- Example from `src/LionFire.Trading.Automation.Bots/AtrBot.cs`:
```csharp
#region Static
public static Type StaticMaterializedType => typeof(AtrBot<TValue>);
#endregion

#region Lifecycle
public PAtrBot() { }
#endregion

#region State
public float OpenScore { get; set; } = 0;
#endregion

#region Event Handling
public override void OnBar() { ... }
#endregion
```

**Namespace Organization:**
- File-scoped namespaces preferred (e.g., `namespace LionFire.Trading.Automation.Bots;`)
- Root namespace: `LionFire.Trading`
- Sub-namespaces follow directory structure
- Hosting extensions use `LionFire.Hosting` namespace

## Import Organization

**Order:**
1. External framework imports (System.*, Microsoft.*)
2. Third-party library imports (QuantConnect.*, Binance.*)
3. LionFire framework imports
4. Project-specific imports

**Global Usings:**
- Common imports in `GlobalUsings.cs` per project
- Example from `src/LionFire.Trading.Indicators/GlobalUsings.cs`:
```csharp
global using Microsoft.Extensions.Logging;
global using System.Threading.Tasks;
global using Microsoft.Extensions.DependencyInjection;
global using System;
global using System.Collections.Generic;
global using LionFire.Trading.DataFlow;
```

**Path Aliases:**
- Not widely used; relative paths preferred

## Parameter System

**Trading Parameters:**
- Use `[TradingParameter]` attribute for bot/indicator parameters
- Properties available: `DefaultValue`, `HardValueMin`, `HardValueMax`, `Step`, `OptimizePriority`
- Location: `src/LionFire.Trading.Abstractions/Structures/Parameters/TradingParameterAttribute.cs`

**Example:**
```csharp
[TradingParameter(
    HardValueMin = 1,
    DefaultMin = 5,
    DefaultMax = 100,
    DefaultValue = 20,
    OptimizerHints = OptimizationDistributionKind.Period)]
public int Period { get; set; } = 20;
```

**Blazor Parameters:**
- Use `[Parameter]` attribute for component parameters
- Located in `.razor.cs` code-behind files

## Error Handling

**Patterns:**
- Use `ArgumentNullException.ThrowIfNull()` for null validation
- Return result types with `IsFilled`, `Reason` properties for operation results
- Async methods return `Task` or `Task<T>`

**Example from FillSimulator:**
```csharp
if (!request.LimitPrice.HasValue)
{
    return new FillResult<decimal>
    {
        IsFilled = false,
        Reason = "Limit price not specified"
    };
}
```

## Logging

**Framework:** Microsoft.Extensions.Logging via ILogger<T>

**Patterns:**
- Inject `ILogger<ClassName>` via constructor
- Use structured logging with message templates
```csharp
ILogger Logger { get; }
public AtrBot(ILogger<AtrBot<TValue>> logger)
{
    Logger = logger;
}

Logger.LogDebug("[AtrBot] OnBar - ATR={ATR}, Size={Size}", ATR?[0], ATR?.Size ?? 0);
```

## Comments

**When to Comment:**
- XML documentation for public APIs, interfaces, and parameters
- Inline comments for complex algorithms or non-obvious behavior
- `// TODO`, `// REVIEW`, `// ENH` markers for future work

**XML Documentation:**
```csharp
/// <summary>
/// Exponential Moving Average (EMA) indicator interface.
/// </summary>
/// <remarks>
/// EMA gives more weight to recent prices using exponential smoothing.
/// Formula: EMA = (Price - Previous EMA) x Multiplier + Previous EMA
/// </remarks>
public interface IEMA<TInput, TOutput> : IIndicator2
```

## Dependency Injection

**Service Registration:**
- Use extension methods on `IServiceCollection`
- Naming: `Add{Feature}()` pattern
- Example from `src/LionFire.Trading.Indicators/Hosting/IndicatorsHostingX.cs`:
```csharp
public static IServiceCollection AddIndicators(this IServiceCollection services)
{
    services
        .AddSingleton<IMarketDataResolver, MarketDataResolver>();
    return services;
}
```

**Configuration:**
- Use `IOptions<T>` pattern
- Configuration sections: `LionFire:Trading:{Feature}`
- Example: `LionFire:Trading:HistoricalData:Windows:BaseDir`

## Generic Constraints

**Numeric Types:**
- Use `where T : struct, INumber<T>` for numeric precision types
- Common type parameters: `TValue`, `TPrecision`, `TPrice`, `TOutput`

**Example:**
```csharp
public class PAtrBot<TValue> : PStandardBot2<PAtrBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
```

## Bot Framework Patterns

**Bot Structure:**
- Parameter class: `P{BotName}<TValue>` - contains configuration
- Implementation class: `{BotName}<TValue>` - contains logic
- Both in same file, parameter class first

**Attributes:**
- `[Bot(Direction = BotDirection.Unidirectional)]` for bot classes
- `[Signal(0)]` for input signals
- `[JsonIgnore]` for computed properties

## Module Design

**Exports:**
- Public types exported, internal implementation hidden
- Extension method classes are `public static`

**Project Organization:**
- Abstractions in separate `*.Abstractions` projects
- Hosting extensions in `Hosting/` folders
- Parameters in `Parameters/` folders

---

*Convention analysis: 2026-01-18*

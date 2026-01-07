# Trading Bot Architect Agent

Expert agent for creating, converting, and maintaining IBot2 trading bots in the LionFire.Trading framework.

## Purpose

This agent handles:
- **New bot creation** from specifications or natural language descriptions
- **cTrader/cAlgo bot conversion** to IBot2 architecture
- **Bot validation** and architecture verification
- **Bot improvement** and optimization guidance

## Bot Architecture Overview

### Class Hierarchies

**Bot Implementation Hierarchy:**
```
IBot2 (interface)
  └─ IBot2<TPrecision>
       └─ IBot2<TParameters, TPrecision>

BotBase2<TParameters, TPrecision>
  └─ Bot2<TParameters, TPrecision>
       └─ SymbolBot2<TParameters, TValue>    # Single symbol focus
            └─ BarsBot2<TParameters, TValue>  # Bar data handling
                 └─ StandardBot2<TParameters, TValue>  # Most bots use this
```

**Parameter Class Hierarchy:**
```
IPBot2 (marker interface)
  └─ PMarketProcessor (abstract base)
       └─ PBot2<TConcrete>
            └─ PSymbolBot2<TConcrete>
                 └─ PBarsBot2<TConcrete, TValue>
                      └─ PStandardBot2<TConcrete, TValue>  # Most bots use this
```

### Key File Locations

| Component | Location |
|-----------|----------|
| IBot2 interface | `/mnt/c/src/Trading/src/LionFire.Trading.Automation/Automation/Bots/IBot2.cs` |
| Bot base classes | `/mnt/c/src/Trading/src/LionFire.Trading.Automation/Automation/Bots/` |
| Parameter bases | `/mnt/c/src/Trading/src/LionFire.Trading.Automation/Automation/Bots/Parameters/` |
| StandardBot2 | `/mnt/c/src/Trading/src/LionFire.Trading.Automation/Automation/Bots/LogicBase/StandardBot2.cs` |
| Exemplar bots | `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/` |
| Indicators | `/mnt/c/src/Trading/src/LionFire.Trading.Indicators.QuantConnect/` |

---

## Core Patterns

### Pattern 1: Parameter Class Structure

Every bot needs a parameter class that:
1. Inherits from `PStandardBot2<TConcrete, TValue>`
2. Uses self-referential generic pattern
3. Implements `MaterializedType` to link to bot class
4. Marks indicator properties with `[PSignal]`
5. Calls `Init()` in parameterized constructor
6. Overrides `InferMissingParameters()` to set `InputLookbacks`

```csharp
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueWindows;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Bots;

[ContainsParameters]
public class P{BotName}<TValue> : PStandardBot2<P{BotName}<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{
    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof({BotName}<TValue>);
    public static Type StaticMaterializedType => typeof({BotName}<TValue>);

    #endregion

    #region Indicator Parameters

    [PSignal]
    public P{IndicatorName}<double, TValue>? {IndicatorName} { get; set; }

    #endregion

    #region Bot Sub-Parameters

    public PUnidirectionalBot? Unidirectional { get; set; }
    public PPointsBot? Points { get; set; }

    #endregion

    #region Trading Parameters

    // Add [TradingParameter] properties here for optimizable values

    #endregion

    #region Lifecycle

    [JsonIgnore]
    const int Lookback = 1;  // How many past values needed

    public P{BotName}() { }

    public P{BotName}(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, uint period)
        : base(exchangeSymbolTimeFrame)
    {
        {IndicatorName} = new P{IndicatorName}<double, TValue>
        {
            Period = (int)period,
            Lookback = Lookback,
        };
        Init();
    }

    protected override void InferMissingParameters()
    {
        InputLookbacks = [
            0,        // Index -1000: Bars (automatic)
            Lookback  // Index 0: First [PSignal] indicator
        ];
        base.InferMissingParameters();
    }

    #endregion

    #region Validation

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull({IndicatorName}, nameof({IndicatorName}));
        ArgumentNullException.ThrowIfNull(Points, nameof(Points));
    }

    #endregion
}
```

### Pattern 2: Bot Class Structure

The bot class:
1. Inherits from `StandardBot2<TParameters, TValue>`
2. Has `[Bot]` attribute specifying direction
3. Has static `ParametersType` property
4. Marks input signals with `[Signal(index)]` matching `[PSignal]` order
5. Implements `OnBar()` with trading logic

```csharp
[Bot(Direction = BotDirection.Unidirectional)]
public class {BotName}<TValue> : StandardBot2<P{BotName}<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    public static Type ParametersType => typeof(P{BotName}<TValue>);

    #region Inputs

    [Signal(0)]
    public IReadOnlyValuesWindow<TValue> {IndicatorName} { get; set; } = null!;

    #endregion

    #region State

    public float OpenScore { get; set; } = 0;
    public float CloseScore { get; set; } = 0;

    #endregion

    #region Event Handling

    public override void OnBar()
    {
        // Guard: ensure enough data
        if ({IndicatorName} == null || {IndicatorName}.Size < 2) return;

        // Access typed parameters
        var typedParams = (P{BotName}<TValue>)Parameters;

        // Calculate signals
        float factor = 0.8f;
        if ({IndicatorName}[0] > {IndicatorName}[1]) OpenScore++;
        else OpenScore *= factor;

        if ({IndicatorName}[0] < {IndicatorName}[1]) CloseScore++;
        else CloseScore *= factor;

        // Entry/Exit logic
        if (CloseScore >= typedParams.Points!.CloseThreshold) { TryClose(); }
        else if (OpenScore >= typedParams.Points.OpenThreshold) { TryOpen(); }

        // Stop Loss / Take Profit
        var sl = Direction switch
        {
            LongAndShort.Long => Bars[0].Low - {IndicatorName}[0],
            LongAndShort.Short => Bars[0].High + {IndicatorName}[0],
            _ => throw new NotImplementedException(),
        };

        Account.SetStopLosses(Symbol, Direction, sl, StopLossFlags.TightenOnly);
    }

    #endregion
}
```

---

## Attribute Reference

### [PSignal] Attribute

Marks parameter properties that represent indicator inputs.

**Location:** Parameter class properties
**Purpose:** Indicates this property will be wired to a `[Signal]` property on the bot

```csharp
[PSignal]
public PAverageTrueRange<double, TValue>? ATR { get; set; }
```

### [Signal(index)] Attribute

Marks bot properties that receive indicator output data.

**Location:** Bot class properties
**Purpose:** Receives indicator values at runtime

```csharp
[Signal(0)]  // Index matches order of [PSignal] properties
public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;
```

**Index Rules:**
- Index 0 = first `[PSignal]` property
- Index 1 = second `[PSignal]` property
- And so on...
- Bars are automatic (inherited from BarsBot2)

### [TradingParameter] Attribute

Marks optimizable trading parameters.

**Location:** Parameter class properties
**Purpose:** Defines optimization ranges and defaults

```csharp
[TradingParameter(
    Description = "Threshold to open position",
    OptimizePriority = -10.0,      // Lower = optimize first (negative recommended)
    DefaultValue = 1,
    HardValueMin = 1,              // Absolute minimum (never exceeded)
    HardValueMax = int.MaxValue,   // Absolute maximum
    ValueMin = 1,                  // Soft minimum for optimization
    ValueMax = 100,                // Soft maximum
    DefaultMin = 1,                // Default optimization range start
    DefaultMax = 30,               // Default optimization range end
    Step = 1,                      // Increment for grid search
    MinStep = 1,                   // Minimum step size
    MaxStep = 4                    // Maximum step size
)]
public int OpenThreshold { get; set; }
```

### [Bot] Attribute

Marks bot classes with metadata.

```csharp
[Bot(Direction = BotDirection.Unidirectional)]
public class MyBot<TValue> : StandardBot2<...>
```

**Direction Options:**
- `BotDirection.Unidirectional` - Long OR short only
- `BotDirection.Bidirectional` - Can hold both long and short

### [ContainsParameters] Attribute

Marks parameter container classes.

```csharp
[ContainsParameters]
public class PMyBot<TValue> : PStandardBot2<...>
```

---

## Available Indicators

### ATR (Average True Range)

```csharp
// Parameter class
[PSignal]
public PAverageTrueRange<double, TValue>? ATR { get; set; }

// Bot class
[Signal(0)]
public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;

// Constructor initialization
ATR = new PAverageTrueRange<double, TValue>
{
    Period = 14,
    Lookback = 1,
    MovingAverageType = MovingAverageType.Wilders,
};
```

### RSI (Relative Strength Index)

```csharp
// Parameter class
[PSignal]
public PRSI<double, TValue>? RSI { get; set; }

// Bot class
[Signal(0)]
public IReadOnlyValuesWindow<TValue> RSI { get; set; } = null!;

// Constructor initialization
RSI = new PRSI<double, TValue>
{
    Period = 14,
    Lookback = 1,
};
```

### ROC (Rate of Change)

```csharp
// Parameter class
[PSignal]
public PROC<double, TValue>? ROC { get; set; }

// Bot class
[Signal(0)]
public IReadOnlyValuesWindow<TValue> ROC { get; set; } = null!;
```

---

## Helper Sub-Parameter Classes

### PPointsBot

Score-based entry/exit system. Add to parameter class:

```csharp
public PPointsBot? Points { get; set; }
```

**Key Properties:**
- `OpenThreshold` - Score needed to open position
- `CloseThreshold` - Score needed to close position
- `IncrementalOpenAmount` - Portion of position to open (0-1)
- `IncrementalCloseAmount` - Portion of position to close (0-1)

### PUnidirectionalBot

Direction control for unidirectional bots:

```csharp
public PUnidirectionalBot? Unidirectional { get; set; }
```

---

## InputLookbacks Array

The `InputLookbacks` array specifies how much historical data each input needs.

**Order:**
1. First element: Bars lookback
2. Subsequent elements: Match `[Signal(index)]` order

```csharp
protected override void InferMissingParameters()
{
    InputLookbacks = [
        0,  // Bars - no extra lookback needed
        1,  // Signal(0) - needs current and 1 previous value
        2   // Signal(1) - needs current and 2 previous values
    ];
    base.InferMissingParameters();
}
```

---

## Inherited Members Reference

### From StandardBot2

| Member | Type | Description |
|--------|------|-------------|
| `Direction` | `LongAndShort` | Trading direction (Long/Short) |
| `TryOpen(amount?)` | Method | Attempt to open position |
| `TryClose(amount?)` | Method | Attempt to close position |
| `OpenPositionPortion(amount?)` | Method | Open position directly |
| `ClosePositionPortion(amount?)` | Method | Close position directly |

### From BarsBot2

| Member | Type | Description |
|--------|------|-------------|
| `Bars` | `IReadOnlyValuesWindow<HLC<TValue>>` | OHLC bar data |
| `Bars[0]` | `HLC<TValue>` | Current bar |
| `Bars[0].High` | `TValue` | Current bar high |
| `Bars[0].Low` | `TValue` | Current bar low |
| `Bars[0].Close` | `TValue` | Current bar close |

### From SymbolBot2

| Member | Type | Description |
|--------|------|-------------|
| `Symbol` | `string` | Symbol being traded |

### From BotBase2

| Member | Type | Description |
|--------|------|-------------|
| `Account` | `IAccount2<TPrecision>` | Account for trading operations |
| `DoubleAccount` | `IAccount2<double>` | Double-precision account |
| `Parameters` | `IPBot2` | Bot parameters |
| `Context` | `IBotContext` | Bot execution context |

---

## Account Operations

### Stop Loss

```csharp
Account.SetStopLosses(Symbol, Direction, price, StopLossFlags.TightenOnly);
```

**Flags:**
- `StopLossFlags.TightenOnly` - Only move SL in profitable direction
- `StopLossFlags.Unspecified` - Allow any movement

### Take Profit

```csharp
Account.SetTakeProfits(Symbol, Direction, price, StopLossFlags.Unspecified);
```

---

## Exemplar Bots

### AtrBot (Simple)

**File:** `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/AtrBot.cs`

Single indicator, score-based entry/exit. Best template for beginners.

**Pattern highlights:**
- One `[PSignal]` property (ATR)
- One `[Signal(0)]` property
- Simple OpenScore/CloseScore state
- Uses Points sub-parameter

### DualAtrBot (Intermediate)

**File:** `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/DualAtrBot.cs`

Two related indicators with comparison logic.

**Pattern highlights:**
- Two `[PSignal]` properties (SlowATR, FastATR)
- Two `[Signal]` properties with indices 0 and 1
- Cross-indicator comparison in OnBar()

### OrderBlocksBot (Advanced)

**File:** `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/OrderBlocksBot.cs`

Complex state machine with multiple signal types.

**Pattern highlights:**
- Custom state management
- Order block detection
- Multiple entry signal types
- ATR-based dynamic SL/TP

---

## cTrader Conversion Rules

### Parameter Mapping

| cTrader | IBot2 |
|---------|-------|
| `[Parameter(DefaultValue = X)]` | `[TradingParameter(DefaultValue = X)]` |
| `MinValue` | `HardValueMin` |
| `MaxValue` | `HardValueMax` |
| Parameter property | Property on parameter class |

### Indicator Mapping

| cTrader | IBot2 Parameter |
|---------|-----------------|
| `Indicators.AverageTrueRange(period)` | `PAverageTrueRange<double, TValue>` |
| `Indicators.ExponentialMovingAverage(...)` | Custom indicator or QuantConnect |
| `Indicators.RelativeStrengthIndex(...)` | `PRSI<double, TValue>` |
| `Indicators.BollingerBands(...)` | Custom implementation |

### Method Mapping

| cTrader | IBot2 |
|---------|-------|
| `OnStart()` | Constructor + `Init()` |
| `OnBar()` | `OnBar()` (similar) |
| `OnTick()` | Not directly supported |
| `OnStop()` | `Stop()` method |
| `ExecuteMarketOrder(...)` | `TryOpen()` / `OpenPositionPortion()` |
| `ClosePosition(...)` | `TryClose()` / `ClosePositionPortion()` |
| `ModifyPosition(sl, tp)` | `Account.SetStopLosses()` + `Account.SetTakeProfits()` |

### Data Access Mapping

| cTrader | IBot2 |
|---------|-------|
| `MarketSeries.Close.Last(0)` | `Bars[0].Close` |
| `MarketSeries.High.Last(1)` | `Bars[1].High` |
| `MarketSeries.Low.Last(2)` | `Bars[2].Low` |
| `Symbol.Bid` | Account context (live only) |
| `Symbol.Ask` | Account context (live only) |
| `Symbol.PipSize` | Symbol configuration |

### Conversion Process

1. **Read cTrader source** - Parse the Robot class
2. **Identify parameters** - Find `[Parameter]` properties
3. **Identify indicators** - Find `Indicators.X()` calls
4. **Map to IBot2**:
   - Create parameter class with `[PSignal]` and `[TradingParameter]`
   - Create bot class with `[Signal]` properties
   - Convert OnBar() logic
5. **Handle unsupported features** - Add TODO comments

---

## Validation Checklist

### Parameter Class

- [ ] Inherits from `PStandardBot2<TConcrete, TValue>`
- [ ] Has `[ContainsParameters]` attribute
- [ ] Has `MaterializedType` property returning bot type
- [ ] All `[PSignal]` properties are nullable
- [ ] Constructor calls `Init()`
- [ ] `InferMissingParameters()` sets `InputLookbacks`
- [ ] Has `ThrowIfInvalid()` validation method

### Bot Class

- [ ] Has `[Bot(Direction = ...)]` attribute
- [ ] Has static `ParametersType` property
- [ ] `[Signal]` indices match `[PSignal]` order (starting from 0)
- [ ] `OnBar()` guards against null/insufficient signals
- [ ] Uses `TryOpen()`/`TryClose()` for position operations
- [ ] Casts `Parameters` to typed version for access

### Signal Pairing

For each `[PSignal]` property in parameter class:
- [ ] Corresponding `[Signal(index)]` exists in bot class
- [ ] Index is sequential starting from 0
- [ ] Property types are compatible

---

## Common Errors

### Signal Mismatch

**Symptom:** Runtime null reference on signal property
**Cause:** `[Signal]` index doesn't match `[PSignal]` order
**Fix:** Verify indices match declaration order

### Missing Init

**Symptom:** InputLookbacks not set, indicators fail
**Cause:** Constructor doesn't call `Init()`
**Fix:** Add `Init()` call at end of parameterized constructor

### Wrong Base Class

**Symptom:** Missing Bars, Symbol, or other properties
**Cause:** Inheriting from wrong base class
**Fix:** Use `StandardBot2` for most bots

### Precision Mismatch

**Symptom:** Type conversion errors
**Cause:** Using `double` instead of `TValue`
**Fix:** Use `TValue.CreateChecked()` for conversions

---

## Bot Complexity Guide

### Simple Bot (like AtrBot)

Use when:
- 1-2 indicators
- Score-based entry/exit
- No complex state
- Single timeframe

### Intermediate Bot (like DualAtrBot)

Use when:
- 2-4 indicators
- Comparative indicator logic
- Simple state tracking
- Using Points sub-parameter

### Advanced Bot (like OrderBlocksBot)

Use when:
- Complex state objects
- Multiple signal types
- Position tracking within bot
- Custom helper methods

---

## Usage Instructions

### Creating a New Bot

1. Determine complexity level (simple/intermediate/advanced)
2. Identify required indicators
3. Create parameter class following the template
4. Create bot class following the template
5. Implement OnBar() logic
6. Validate against checklist
7. Build and test

### Converting a cTrader Bot

1. Read and analyze cTrader source
2. List all parameters and their types
3. List all indicators used
4. Check indicator availability in IBot2
5. Map using conversion rules
6. Generate parameter and bot classes
7. Flag unsupported features with TODO
8. Validate and test

### Output Location

Place generated bot files in:
```
/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/{BotName}Bot.cs
```

For proprietary bots:
```
/mnt/c/src/Trading.Proprietary/src/LionFire.Trading.Proprietary.Bots/{BotName}Bot.cs
```

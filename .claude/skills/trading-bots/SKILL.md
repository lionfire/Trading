# Trading Bot Development Skill

Discover and integrate trading bot creation capabilities for the LionFire.Trading platform.

## When to Use This Skill

Use when:
- Creating new trading bots for the LionFire.Trading platform
- Converting existing cTrader/cAlgo bots to IBot2 architecture
- Understanding the bot architecture and patterns
- Troubleshooting bot implementation issues
- Learning the IBot2 framework

## Quick Start

### Create a New Bot

```
/trading:create-bot MyStrategy --indicators ATR,RSI --complexity intermediate
```

### Convert a cTrader Bot

```
/trading:convert-ctrader-bot /path/to/MyCTraderBot.cs
```

## Core Concepts

The IBot2 architecture separates concerns into two classes:

| Class Type | Naming | Purpose |
|------------|--------|---------|
| **Parameter Class** | `P{BotName}<TValue>` | Configuration, indicators, optimization ranges |
| **Bot Class** | `{BotName}<TValue>` | Runtime logic, signal processing, trading decisions |

**Key principle:** Parameters define "what" (configuration), Bots define "how" (behavior).

## Architecture Overview

```
Parameter Hierarchy:
PMarketProcessor → PBot2 → PSymbolBot2 → PBarsBot2 → PStandardBot2

Bot Hierarchy:
Bot2 → SymbolBot2 → BarsBot2 → StandardBot2
```

Most bots inherit from `StandardBot2` (bot) and `PStandardBot2` (parameters).

## Key Attributes

| Attribute | Location | Purpose |
|-----------|----------|---------|
| `[PSignal]` | Parameter class | Marks indicator parameter properties |
| `[Signal(index)]` | Bot class | Receives indicator data at runtime |
| `[TradingParameter]` | Parameter class | Marks optimizable parameters |
| `[Bot(Direction=...)]` | Bot class | Bot metadata (direction) |
| `[ContainsParameters]` | Parameter class | Marks parameter containers |

## Available Commands

### /trading:create-bot

Create a new IBot2 trading bot from specification.

```
/trading:create-bot <name> [--indicators X,Y] [--complexity simple|intermediate|advanced]
```

**Details:** `.claude/commands/trading/create-bot.md`

### /trading:convert-ctrader-bot

Convert an existing cTrader/cAlgo bot to IBot2 architecture.

```
/trading:convert-ctrader-bot <source-file> [--output-name Name]
```

**Details:** `.claude/commands/trading/convert-ctrader-bot.md`

## Workflow Examples

### Workflow 1: Create Simple Indicator Bot

```bash
# Step 1: Create the bot
/trading:create-bot RsiOverbought --indicators RSI --complexity simple

# Step 2: Review generated code
# Edit /mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/RsiOverboughtBot.cs

# Step 3: Implement trading logic in OnBar()

# Step 4: Build to verify
dotnet-win build /mnt/c/src/Internal/LionFire.All.Trading.slnf
```

### Workflow 2: Create Multi-Indicator Bot

```bash
# Step 1: Create intermediate complexity bot
/trading:create-bot TrendMomentum --indicators ATR,RSI --complexity intermediate

# Step 2: Customize trading logic
# - Compare ATR for volatility
# - Use RSI for overbought/oversold
# - Combine signals for entry/exit

# Step 3: Configure optimization parameters
# Adjust [TradingParameter] ranges in parameter class
```

### Workflow 3: Convert cTrader Bot

```bash
# Step 1: Locate cTrader source
ls /mnt/c/src/Trading.Proprietary/src/LionFire.Trading.Proprietary.cAlgo/

# Step 2: Convert
/trading:convert-ctrader-bot /path/to/MyCTraderBot.cs

# Step 3: Review conversion
# - Check TODO comments for unsupported features
# - Verify indicator mappings
# - Test signal logic

# Step 4: Build and backtest
dotnet-win build /mnt/c/src/Internal/LionFire.All.Trading.slnf
```

### Workflow 4: Add Indicator to Existing Bot

```bash
# Step 1: Add [PSignal] to parameter class
[PSignal]
public PRSI<double, TValue>? RSI { get; set; }

# Step 2: Add [Signal] to bot class (next available index)
[Signal(1)]  // After existing Signal(0)
public IReadOnlyValuesWindow<TValue> RSI { get; set; } = null!;

# Step 3: Update InputLookbacks array
InputLookbacks = [0, 1, 1];  // Bars, ATR, RSI

# Step 4: Initialize in constructor
RSI = new PRSI<double, TValue> { Period = 14, Lookback = 1 };

# Step 5: Use in OnBar()
if (RSI[0] > TValue.CreateChecked(70)) { /* overbought logic */ }
```

## Existing Exemplars

Study these bots to understand patterns:

| Bot | Location | Complexity | Learn From |
|-----|----------|------------|------------|
| AtrBot | `LionFire.Trading.Automation.Bots/AtrBot.cs` | Simple | Single indicator, score-based entry |
| DualAtrBot | `LionFire.Trading.Automation.Bots/DualAtrBot.cs` | Intermediate | Multiple indicators, comparison |
| OrderBlocksBot | `LionFire.Trading.Automation.Bots/OrderBlocksBot.cs` | Advanced | Complex state, custom signals |

**Base path:** `/mnt/c/src/Trading/src/`

## Available Indicators

| Indicator | Parameter Type | Use Case |
|-----------|---------------|----------|
| ATR | `PAverageTrueRange<double, TValue>` | Volatility, stop loss sizing |
| RSI | `PRSI<double, TValue>` | Overbought/oversold |
| ROC | `PROC<double, TValue>` | Rate of change, momentum |

## Best Practices

1. **Start Simple** - Begin with AtrBot as template, add complexity incrementally
2. **Test Incrementally** - Build after each change to catch errors early
3. **Use Type Safety** - Leverage `TValue` generic for precision flexibility
4. **Document Parameters** - Use XML comments and TradingParameter descriptions
5. **Validate Early** - Implement `ThrowIfInvalid()` with comprehensive checks
6. **Guard OnBar()** - Always check signal availability before using

## Common Patterns

### Score-Based Entry/Exit

```csharp
public float OpenScore { get; set; } = 0;

public override void OnBar()
{
    if (Condition) OpenScore++;
    else OpenScore *= 0.8f;

    if (OpenScore >= typedParams.Points!.OpenThreshold)
        TryOpen();
}
```

### ATR-Based Stop Loss

```csharp
var sl = Direction switch
{
    LongAndShort.Long => Bars[0].Low - ATR[0],
    LongAndShort.Short => Bars[0].High + ATR[0],
    _ => throw new NotImplementedException(),
};
Account.SetStopLosses(Symbol, Direction, sl, StopLossFlags.TightenOnly);
```

### Indicator Comparison

```csharp
// Fast/Slow crossover pattern
if (FastIndicator[0] > SlowIndicator[0] && FastIndicator[1] <= SlowIndicator[1])
{
    // Bullish crossover
    TryOpen();
}
```

## Troubleshooting

### Bot not receiving indicator data

**Symptoms:** Null reference, empty signal window
**Check:**
- `[Signal]` indices match `[PSignal]` order
- `InputLookbacks` includes entry for each signal
- `Init()` is called in constructor

### Optimization not finding parameters

**Symptoms:** Parameters not varied during optimization
**Check:**
- `[TradingParameter]` attribute is present
- `OptimizePriority` is not too negative
- Parameter type is supported (numeric)

### cTrader conversion missing features

**Symptoms:** TODO comments in converted code
**Resolution:**
- `OnTick()` - Use OnBar or implement custom solution
- Pending orders - Manual implementation needed
- Custom indicators - May need porting

## Reference Documentation

| Resource | Path |
|----------|------|
| Agent (full implementation) | `.claude/agents/trading/bot-architect.md` |
| Create command | `.claude/commands/trading/create-bot.md` |
| Convert command | `.claude/commands/trading/convert-ctrader-bot.md` |
| Exemplar bots | `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/` |
| Bot base classes | `/mnt/c/src/Trading/src/LionFire.Trading.Automation/Automation/Bots/` |
| Parameter bases | `.../Automation/Bots/Parameters/` |
| Signal attributes | `/mnt/c/src/Trading/src/LionFire.Trading.Abstractions/DataFlow/Signals/` |
| Trading parameters | `.../Abstractions/Structures/Parameters/TradingParameterAttribute.cs` |

## Integration Points

### Backtesting

- Bots run via `BotHarness` for backtesting
- Parameter optimization uses `[TradingParameter]` ranges
- Results in optimization dashboard

### Live Trading

- Deploy via Orleans `RealtimeBotHarnessG` grain
- Same code runs backtest and live
- Context provides live account/market data

### Dashboard

- FireLynx.Blazor.Internal displays bot status
- Real-time position and P&L tracking
- Bot log streaming

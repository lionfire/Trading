# /trading:convert-ctrader-bot

Convert a cTrader/cAlgo robot to IBot2 architecture.

## Usage

```
/trading:convert-ctrader-bot <source-file> [options]
```

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `source-file` | Yes | Path to cTrader .cs file |
| `--output-name` | No | Override output bot name (default: derived from source) |
| `--output-path` | No | Custom output path |
| `--preserve-comments` | No | Keep original comments in converted code |
| `--proprietary` | No | Output to Trading.Proprietary repo |

## Examples

### Basic conversion
```
/trading:convert-ctrader-bot /path/to/MyRobot.cs
```

### With custom name
```
/trading:convert-ctrader-bot /path/to/MyRobot.cs --output-name ImprovedRobot
```

### To proprietary repo
```
/trading:convert-ctrader-bot /path/to/MyRobot.cs --proprietary
```

## Conversion Process

### Step 1: Parse cTrader Source

Extract from the cTrader Robot class:
- Class name and inheritance
- `[Parameter]` properties
- `Indicators.X()` calls
- `OnStart()`, `OnBar()`, `OnTick()`, `OnStop()` methods
- Position operations (`ExecuteMarketOrder`, `ClosePosition`, etc.)
- Data access patterns (`MarketSeries`, `Symbol`, etc.)

### Step 2: Map Parameters

| cTrader | IBot2 |
|---------|-------|
| `[Parameter("Description", DefaultValue = X)]` | `[TradingParameter(Description, DefaultValue = X)]` |
| `MinValue = X` | `HardValueMin = X` |
| `MaxValue = X` | `HardValueMax = X` |
| `Step = X` | `Step = X` |

### Step 3: Map Indicators

| cTrader Indicator | IBot2 Parameter Type |
|-------------------|---------------------|
| `Indicators.AverageTrueRange(period, maType)` | `PAverageTrueRange<double, TValue>` |
| `Indicators.RelativeStrengthIndex(source, period)` | `PRSI<double, TValue>` |
| `Indicators.RateOfChange(source, period)` | `PROC<double, TValue>` |
| `Indicators.ExponentialMovingAverage(...)` | Custom or QuantConnect equivalent |
| `Indicators.SimpleMovingAverage(...)` | Custom or QuantConnect equivalent |
| `Indicators.BollingerBands(...)` | Custom implementation needed |
| `Indicators.MacdHistogram(...)` | Custom implementation needed |

### Step 4: Map Methods

| cTrader Method | IBot2 Equivalent |
|----------------|------------------|
| `OnStart()` | Constructor + `Init()` |
| `OnBar()` | `OnBar()` (similar pattern) |
| `OnTick()` | **Not directly supported** - add TODO |
| `OnStop()` | `Stop()` method |
| `ExecuteMarketOrder(type, symbol, volume, label, sl, tp)` | `TryOpen()` or `OpenPositionPortion()` |
| `ClosePosition(position)` | `TryClose()` or `ClosePositionPortion()` |
| `ModifyPosition(position, sl, tp)` | `Account.SetStopLosses()` + `Account.SetTakeProfits()` |

### Step 5: Map Data Access

| cTrader Data | IBot2 Equivalent |
|--------------|------------------|
| `MarketSeries.Close.Last(0)` | `Bars[0].Close` |
| `MarketSeries.High.Last(1)` | `Bars[1].High` |
| `MarketSeries.Low.Last(2)` | `Bars[2].Low` |
| `MarketSeries.Open.Last(0)` | `Bars[0].Open` (if using OHLC) |
| `Symbol.Bid` | Account context (live only) |
| `Symbol.Ask` | Account context (live only) |
| `Symbol.PipSize` | Symbol configuration |
| `Symbol.TickSize` | Symbol configuration |
| `Server.Time` | Context time |
| `Positions` | `Account.Positions` |

### Step 6: Generate IBot2 Code

Create parameter class and bot class following the patterns in the bot-architect agent.

### Step 7: Flag Unsupported Features

Add TODO comments for:
- `OnTick()` handlers (requires different approach)
- Custom indicators without IBot2 equivalents
- Platform-specific features (Print, Chart objects, etc.)
- Pending orders (limit orders, stop orders)

## Output

### Generated Files

**Single file containing both classes:**
```
{OutputPath}/{BotName}Bot.cs
```

### Conversion Report

After conversion, a summary is provided:
- Converted parameters count
- Converted indicators count
- Unsupported features (with TODO markers)
- Manual review items

## Unsupported Features

The following cTrader features are not directly supported and will be flagged:

| Feature | Handling |
|---------|----------|
| `OnTick()` | TODO comment - use OnBar or request enhancement |
| Pending orders | TODO comment - manual implementation needed |
| Chart objects | Removed - not applicable |
| `Print()` | Convert to logging |
| Custom indicators | TODO comment - may need porting |
| Multi-symbol | TODO comment - requires different bot pattern |

## Post-Conversion Steps

1. **Review TODO comments** - Address unsupported features
2. **Verify indicator mappings** - Ensure correct IBot2 indicators used
3. **Test signal logic** - Compare with original cTrader behavior
4. **Adjust parameters** - Fine-tune `[TradingParameter]` ranges
5. **Build and test** - Run backtests to validate

## Example Conversion

### cTrader Input
```csharp
[Robot(AccessRights = AccessRights.None)]
public class SimpleAtrBot : Robot
{
    [Parameter("ATR Period", DefaultValue = 14, MinValue = 1)]
    public int AtrPeriod { get; set; }

    [Parameter("Open Threshold", DefaultValue = 3)]
    public int OpenThreshold { get; set; }

    private AverageTrueRange _atr;
    private int _openScore = 0;

    protected override void OnStart()
    {
        _atr = Indicators.AverageTrueRange(AtrPeriod, MovingAverageType.Exponential);
    }

    protected override void OnBar()
    {
        if (_atr.Result.Last(0) > _atr.Result.Last(1))
            _openScore++;
        else
            _openScore = 0;

        if (_openScore >= OpenThreshold)
            ExecuteMarketOrder(TradeType.Buy, Symbol, 10000);
    }
}
```

### IBot2 Output
```csharp
// Converted from cTrader: SimpleAtrBot
// TODO: Review and adjust trading logic

public class PSimpleAtrBot<TValue> : PStandardBot2<PSimpleAtrBot<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{
    [JsonIgnore]
    public override Type MaterializedType => typeof(SimpleAtrBot<TValue>);
    public static Type StaticMaterializedType => typeof(SimpleAtrBot<TValue>);

    [PSignal]
    public PAverageTrueRange<double, TValue>? ATR { get; set; }

    public PPointsBot? Points { get; set; }

    [TradingParameter(DefaultValue = 14, HardValueMin = 1)]
    public int AtrPeriod { get; set; } = 14;

    [JsonIgnore]
    const int Lookback = 1;

    public PSimpleAtrBot() { }

    public PSimpleAtrBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, int atrPeriod = 14)
        : base(exchangeSymbolTimeFrame)
    {
        AtrPeriod = atrPeriod;
        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = atrPeriod,
            Lookback = Lookback,
            MovingAverageType = MovingAverageType.Exponential,
        };
        Init();
    }

    protected override void InferMissingParameters()
    {
        InputLookbacks = [0, Lookback];
        base.InferMissingParameters();
    }
}

[Bot(Direction = BotDirection.Unidirectional)]
public class SimpleAtrBot<TValue> : StandardBot2<PSimpleAtrBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    public static Type ParametersType => typeof(PSimpleAtrBot<TValue>);

    [Signal(0)]
    public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;

    public int OpenScore { get; set; } = 0;

    public override void OnBar()
    {
        if (ATR == null || ATR.Size < 2) return;

        var typedParams = (PSimpleAtrBot<TValue>)Parameters;

        if (ATR[0] > ATR[1])
            OpenScore++;
        else
            OpenScore = 0;

        if (OpenScore >= typedParams.Points!.OpenThreshold)
            TryOpen();
    }
}
```

## See Also

- **Agent:** `.claude/agents/trading/bot-architect.md`
- **Creation:** `/trading:create-bot`
- **Skill:** `.claude/skills/trading-bots/SKILL.md`
- **cTrader sources:** `/mnt/c/src/Trading.Proprietary/src/LionFire.Trading.Proprietary.cAlgo/`

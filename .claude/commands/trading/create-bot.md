# /trading:create-bot

Create a new IBot2 trading bot from specification.

## Usage

```
/trading:create-bot <bot-name> [options]
```

## Parameters

| Parameter | Required | Description |
|-----------|----------|-------------|
| `bot-name` | Yes | Name for the bot (e.g., "RsiMomentum", "VolatilityBreakout") |
| `--indicators` | No | Comma-separated indicator list (e.g., "ATR,RSI") |
| `--complexity` | No | simple, intermediate, or advanced (default: simple) |
| `--description` | No | Bot description for documentation |
| `--direction` | No | unidirectional or bidirectional (default: unidirectional) |
| `--proprietary` | No | If set, create in Trading.Proprietary repo |

## Examples

### Simple ATR-based bot
```
/trading:create-bot VolatilityBreakout --indicators ATR --complexity simple
```

### Multi-indicator bot
```
/trading:create-bot TrendFollower --indicators ATR,RSI --complexity intermediate
```

### With full options
```
/trading:create-bot MeanReversion --indicators ATR,RSI \
  --complexity intermediate \
  --description "Mean reversion strategy using ATR for volatility and RSI for overbought/oversold" \
  --direction unidirectional
```

### Proprietary bot
```
/trading:create-bot MySecretStrategy --indicators ATR --proprietary
```

## Process

1. **Parse specification** - Extract bot name, indicators, and options
2. **Determine complexity** - Based on indicator count and options
3. **Read exemplar** - Load appropriate exemplar (AtrBot, DualAtrBot, OrderBlocksBot)
4. **Generate parameter class** - Create `P{BotName}<TValue>` with:
   - `[PSignal]` properties for each indicator
   - `MaterializedType` linking to bot class
   - `InferMissingParameters()` with `InputLookbacks`
   - `ThrowIfInvalid()` validation
5. **Generate bot class** - Create `{BotName}<TValue>` with:
   - `[Signal(index)]` properties matching indicators
   - `OnBar()` with placeholder logic
   - State variables for scoring
6. **Validate** - Check against validation checklist
7. **Write file** - Save to appropriate location

## Output Location

**Public bots:**
```
/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/{BotName}Bot.cs
```

**Proprietary bots:**
```
/mnt/c/src/Trading.Proprietary/src/LionFire.Trading.Proprietary.Bots/{BotName}Bot.cs
```

## Available Indicators

| Indicator | Parameter Type | Description |
|-----------|---------------|-------------|
| ATR | `PAverageTrueRange<double, TValue>` | Average True Range |
| RSI | `PRSI<double, TValue>` | Relative Strength Index |
| ROC | `PROC<double, TValue>` | Rate of Change |

## Complexity Levels

### Simple
- 1 indicator
- Score-based entry/exit using PPointsBot
- Template: AtrBot

### Intermediate
- 2-3 indicators
- Comparative logic between indicators
- Template: DualAtrBot

### Advanced
- 3+ indicators or complex state
- Custom state management
- Multiple signal types
- Template: OrderBlocksBot

## Post-Creation Steps

After the bot is created:

1. **Review generated code** - Check the OnBar() placeholder logic
2. **Implement trading logic** - Replace placeholder with actual strategy
3. **Configure parameters** - Adjust `[TradingParameter]` ranges
4. **Build** - `dotnet-win build /mnt/c/src/Internal/LionFire.All.Trading.slnf`
5. **Test** - Run backtests to validate

## See Also

- **Agent:** `.claude/agents/trading/bot-architect.md`
- **Conversion:** `/trading:convert-ctrader-bot`
- **Skill:** `.claude/skills/trading-bots/SKILL.md`
- **Exemplars:** `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Bots/`

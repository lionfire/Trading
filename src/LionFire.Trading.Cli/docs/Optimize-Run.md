# lft optimize run

Run a grid-search parameter optimization for a trading bot using historical data.

## Quick Start

```bash
# Basic: optimize LorentzianDynamicBot on BTCUSDT m1 for the last month
lft optimize run -b LorentzianDynamicBot --symbol BTCUSDT --timeframe m1

# Custom date range with more backtests
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --from 2025-08-01 --to 2026-02-01 --max-backtests 10000

# Include lower-priority parameters (e.g., StopLossATRMultiplier)
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --max-backtests 10000 --min-priority -1

# Control thread count (default: auto = CPU count)
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 --threads 10
```

## Options

### Bot and Market

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--bot` | `-b` | (required) | Bot type: `LorentzianDynamicBot`, `PAtrBot` |
| `--symbol` | `-s` | `BTCUSDT` | Trading symbol |
| `--exchange` | `-e` | `Binance` | Exchange |
| `--area` | `-a` | `futures` | Exchange area (`spot`, `futures`) |
| `--timeframe` | `-t` | `h1` | Bar timeframe (`m1`, `m5`, `m15`, `m30`, `h1`, `h2`, `h4`, `d1`, etc.) |

### Date Range

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--from` | `-f` | 1 month ago | Start date (inclusive, snapped to bar boundary) |
| `--to` | | now | End date (exclusive, snapped to bar boundary) |

### Optimization Control

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--max-backtests` | `-m` | `50000` | Maximum backtests to run. The level-of-detail system coarsens the parameter grid to fit within this budget. |
| `--min-priority` | | `0` | Minimum parameter priority to include. Lower values add more parameters (e.g., `-1` adds StopLossATRMultiplier, `-5` adds TakeProfitATRMultiplier). |
| `--batch-size` | | `1024` | Max backtests per batch |
| `--threads` | `-n` | `0` (auto) | Parallel threads. `0` = CPU count, `1` = sequential, `N` = N threads. Optimal: ~10 for 12-core. |

### Output

| Option | Alias | Default | Description |
|--------|-------|---------|-------------|
| `--progress-interval` | `-p` | `5` | Progress update interval in seconds |
| `--json` | | `false` | Machine-parseable JSON-lines output |
| `--quiet` | `-q` | `false` | Suppress progress output |
| `--journals` | `-j` | `false` | Save trade journals for top results |
| `--keep-journals` | | `5` | Number of top results to keep journals for |

### Config Files

| Option | Alias | Description |
|--------|-------|-------------|
| `--config` | `-c` | Path to HJSON/JSON config file |
| `--preset` | | Named preset from `~/.lft/presets/` |

Precedence: Command line > Config file > Preset > Defaults.

## Config File Format

Config files use HJSON (or JSON). Example (`~/.lft/presets/lorentzian-btc.hjson`):

```hjson
bot: LorentzianDynamicBot
symbol: BTCUSDT
exchange: Binance
area: futures
timeframe: m1

optimization:
{
  maxBacktests: 10000
  minParameterPriority: -1
}
```

## How It Works

### Parameter Grid

Each bot has parameters marked with `[TradingParameter]` attributes that define:
- **Priority** (`OptimizePriority`): Higher priority = included first. Use `--min-priority` to control which levels are included.
- **Range** (`DefaultMin`, `DefaultMax`, `Step`): Defines the search space.

The level-of-detail system automatically coarsens the grid to fit within `--max-backtests`:
- At level 0 (full resolution), each parameter uses its full step size
- At negative levels, parameters are sampled more coarsely (halving test points per level)
- The system picks the finest level whose total permutations fit the budget

### LorentzianDynamicBot Parameter Priorities

| Priority | Parameters | Typical count at level 0 |
|----------|-----------|-------------------------|
| 0 | MinSignalStrength, MinLorentzianConfidence, HoldBars | ~2,700 |
| -1 | StopLossATRMultiplier | x9 |
| -5 | TakeProfitATRMultiplier, MinWinRate, WinRateWindow, ... | x many |
| -10 to -50 | Regime detection tuning, k-NN parameters | x many |

With `--max-backtests 10000`:
- Default (`--min-priority 0`): ~648 backtests
- With `--min-priority -1`: ~288 backtests (different level chosen to fit budget)

### Execution Pipeline

1. Load historical bar data for the symbol/timeframe/date range
2. Enumerate parameter combinations from the grid
3. Run backtests in parallel batches (controlled by `--threads`)
4. Write results to `backtests.csv` and grid level files
5. Calculate optimization score

### Output Directory

Results are written to:
```
/st/Investing-Output/.local/Backtests/{BotName}/{Symbol}/{Timeframe}/{StartDate}-{EndDate}/{Exchange}/{RunNumber}/
```

Contents:
- `backtests.csv` - All backtest results
- `OptimizationRunInfo.hjson` - Run configuration
- `OptimizationScore.hjson` - Scoring summary
- `GridLevel N.hjson` - Parameter grid at each level of detail

## Examples

### Quick test (small grid, short period)
```bash
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --from 2026-02-01 --to 2026-02-15 --max-backtests 500 -p 3
```

### Production run (6 months, expanded parameters)
```bash
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --from 2025-08-01 --to 2026-02-01 \
  --max-backtests 10000 --min-priority -1 --threads 10 -p 10
```

### Machine-parseable output for scripting
```bash
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --max-backtests 1000 --json -q 2>/dev/null | jq '.status'
```

### With trade journals for analysis
```bash
lft optimize run -b LorentzianDynamicBot -s BTCUSDT -t m1 \
  --max-backtests 1000 --journals --keep-journals 10
```

## Performance Notes

Benchmarked on 12-core / 23-thread machine (2026-03-12):

| Workload | 10 threads | 20 threads |
|----------|-----------|-----------|
| 288bt x 2 weeks | 16s | 16s |
| 648bt x 2 weeks | 33s | 33s |
| 288bt x 6 months | 3m33s | 4m00s |
| 648bt x 6 months | 6m07s | 6m20s |

- **10 threads is optimal** for 12-core machines. 20 threads adds contention overhead.
- Short runs are dominated by data loading (~8-40s depending on date range).
- Profile breakdown (LorentzianDynamicBot): Lorentzian k-NN 77%, Regime Detection 14%, Signal Validation 3%.

## Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Error (invalid args, missing data, etc.) |
| 130 | Cancelled (Ctrl+C / SIGINT) |

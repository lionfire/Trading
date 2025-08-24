# Optimization Queue CLI Commands

The LionFire Trading CLI provides comprehensive command-line access to the distributed optimization queue system. These commands allow you to submit, monitor, and manage optimization jobs across Orleans silos.

## Prerequisites

- Orleans silo must be running with the optimization queue configured
- CLI must have Orleans client connectivity to the silo cluster
- Proper authentication/authorization if configured

## Configuration

The CLI connects to Orleans using configuration from:
- `appsettings.json` in the CLI project
- Environment variables
- Command-line parameters (Orleans client configuration)

### Orleans Client Configuration

By default, the CLI uses localhost clustering. For production environments, configure Orleans connection in `appsettings.json`:

```json
{
  "Orleans": {
    "ConnectionString": "your-orleans-connection-string",
    "ClusterId": "your-cluster-id",
    "ServiceId": "trading-service"
  }
}
```

## Commands Overview

All queue commands are under the `optimize` area with `queue` subcommands:

- `optimize queue add` - Submit new optimization job
- `optimize queue list` - List jobs in queue
- `optimize queue status` - Get detailed job status
- `optimize queue cancel` - Cancel running or queued job

## Command Reference

### `optimize queue add`

Submit a new optimization job to the distributed queue.

#### Syntax
```bash
lionfire-trading optimize queue add [options]
```

#### Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `--priority` | Job priority (lower = higher priority) | 5 | No |
| `--config-file` | JSON file with optimization parameters | - | No |
| `--bot-type` | Bot type to optimize | - | No |
| `--exchange` | Exchange name | "Binance" | No |
| `--exchange-area` | Exchange area (spot/futures) | "futures" | No |
| `--symbol` | Trading symbol | "BTCUSDT" | No |
| `--interval` | Time frame | "h1" | No |
| `--from` | Start date for backtest | 30 days ago | No |
| `--to` | End date for backtest | Now | No |
| `--json` | Output result as JSON | false | No |

#### Examples

**Submit job with config file:**
```bash
lionfire-trading optimize queue add --config-file bot-config.json --priority 3
```

**Submit basic job with CLI parameters:**
```bash
lionfire-trading optimize queue add \
  --symbol ETHUSDT \
  --interval h4 \
  --from "2024-01-01" \
  --to "2024-12-31" \
  --priority 1
```

**Submit job and get JSON output:**
```bash
lionfire-trading optimize queue add --config-file config.json --json
```

#### Config File Format

The `--config-file` should contain a JSON representation of `PMultiSim` optimization parameters:

```json
{
  "ExchangeSymbolTimeFrame": {
    "Exchange": "Binance",
    "ExchangeArea": "futures",
    "Symbol": "BTCUSDT",
    "TimeFrame": "1h"
  },
  "Start": "2024-01-01T00:00:00Z",
  "EndExclusive": "2024-12-31T00:00:00Z",
  "BotParameters": {
    "BotType": "MyTradingBot",
    "Parameters": {
      "FastPeriod": { "Min": 5, "Max": 20, "Step": 1 },
      "SlowPeriod": { "Min": 20, "Max": 50, "Step": 5 },
      "StopLoss": 0.02,
      "TakeProfit": 0.04
    }
  },
  "OptimizationSettings": {
    "Strategy": "GridSearch",
    "MaxConcurrentJobs": 4,
    "RandomSeed": 12345
  }
}
```

#### Success Output
```
Job queued successfully!
Job ID: a1b2c3d4-5e6f-7890-abcd-ef1234567890
Priority: 3
Status: Queued
```

#### JSON Output
```json
{
  "JobId": "a1b2c3d4-5e6f-7890-abcd-ef1234567890",
  "Priority": 3,
  "Status": "Queued"
}
```

---

### `optimize queue list`

List optimization jobs in the queue with filtering and formatting options.

#### Syntax
```bash
lionfire-trading optimize queue list [options]
```

#### Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `--status` | Filter by status (queued/running/completed/failed/cancelled) | All | No |
| `--limit` | Maximum number of jobs to show | 50 | No |
| `--json` | Output as JSON | false | No |
| `--summary` | Show only queue summary | false | No |

#### Examples

**List all jobs:**
```bash
lionfire-trading optimize queue list
```

**List only running jobs:**
```bash
lionfire-trading optimize queue list --status running
```

**Get queue summary:**
```bash
lionfire-trading optimize queue list --summary
```

**List recent completed jobs as JSON:**
```bash
lionfire-trading optimize queue list --status completed --limit 10 --json
```

#### Table Output
```
┌─────────────┬───────────┬──────────┬─────────────┬──────────┬──────────┬──────────────┐
│ Job ID      │ Status    │ Priority │ Created     │ Duration │ Progress │ Submitted By │
├─────────────┼───────────┼──────────┼─────────────┼──────────┼──────────┼──────────────┤
│ a1b2c3d4... │ Running   │ 3        │ 2 hrs ago   │ 1:45:23  │ 67.3%    │ CLI          │
│ b2c3d4e5... │ Queued    │ 5        │ 1 hr ago    │ -        │ -        │ WebUI        │
│ c3d4e5f6... │ Completed │ 1        │ 3 hrs ago   │ 2:15:45  │ 100%     │ CLI          │
└─────────────┴───────────┴──────────┴─────────────┴──────────┴──────────┴──────────────┘
Showing 3 job(s)
```

#### Summary Output
```
┌─────────────────┬───────┐
│ Metric          │ Value │
├─────────────────┼───────┤
│ Queued Jobs     │ 5     │
│ Running Jobs    │ 2     │
│ Completed Jobs  │ 25    │
│ Failed Jobs     │ 1     │
│ Cancelled Jobs  │ 0     │
│ Active Silos    │ 3     │
│ Avg Duration    │ 1h 23m│
└─────────────────┴───────┘
```

---

### `optimize queue status`

Get detailed status information for a specific optimization job.

#### Syntax
```bash
lionfire-trading optimize queue status --job-id <job-id> [options]
```

#### Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `--job-id` | Job ID to check | - | Yes |
| `--json` | Output as JSON | false | No |

#### Examples

**Get job status:**
```bash
lionfire-trading optimize queue status --job-id a1b2c3d4-5e6f-7890-abcd-ef1234567890
```

**Get status as JSON:**
```bash
lionfire-trading optimize queue status --job-id a1b2c3d4-5e6f-7890-abcd-ef1234567890 --json
```

#### Table Output
```
┌─────────────────┬────────────────────────────────┐
│ Property        │ Value                          │
├─────────────────┼────────────────────────────────┤
│ Job ID          │ a1b2c3d4-5e6f-7890-abcd-ef... │
│ Status          │ Running                        │
│ Priority        │ 3                              │
│ Created         │ 2024-01-15 14:30:00 UTC       │
│ Started         │ 2024-01-15 14:31:00 UTC       │
│ Submitted By    │ CLI                            │
│ Duration        │ 1 hour 45 minutes             │
│ Progress        │ 67.3% (1,346/2,000)          │
│ Est. Completion │ 52 minutes                     │
│ Assigned Silo   │ silo-worker-01                 │
└─────────────────┴────────────────────────────────┘
```

---

### `optimize queue cancel`

Cancel a queued or running optimization job.

#### Syntax
```bash
lionfire-trading optimize queue cancel --job-id <job-id> [options]
```

#### Parameters

| Parameter | Description | Default | Required |
|-----------|-------------|---------|----------|
| `--job-id` | Job ID to cancel | - | Yes |
| `--json` | Output result as JSON | false | No |

#### Examples

**Cancel a job:**
```bash
lionfire-trading optimize queue cancel --job-id a1b2c3d4-5e6f-7890-abcd-ef1234567890
```

**Cancel with JSON output:**
```bash
lionfire-trading optimize queue cancel --job-id a1b2c3d4-5e6f-7890-abcd-ef1234567890 --json
```

#### Success Output
```
Job a1b2c3d4-5e6f-7890-abcd-ef1234567890 cancelled successfully
```

#### JSON Output
```json
{
  "Success": true,
  "JobId": "a1b2c3d4-5e6f-7890-abcd-ef1234567890"
}
```

## Job Status Values

| Status | Description |
|--------|-------------|
| `Queued` | Job is waiting in queue to be executed |
| `Running` | Job is currently being processed by a silo |
| `Completed` | Job finished successfully |
| `Failed` | Job encountered an error during execution |
| `Cancelled` | Job was cancelled by user request |

## Scripting and Automation

### JSON Output

All commands support `--json` flag for machine-readable output, making them suitable for:
- Bash scripts and automation
- CI/CD pipelines
- Monitoring systems
- Integration with other tools

### Example Automation Script

```bash
#!/bin/bash

# Submit optimization job
RESULT=$(lionfire-trading optimize queue add --config-file $1 --json)
JOB_ID=$(echo $RESULT | jq -r '.JobId')

echo "Submitted job: $JOB_ID"

# Monitor progress
while true; do
    STATUS=$(lionfire-trading optimize queue status --job-id $JOB_ID --json)
    STATE=$(echo $STATUS | jq -r '.Status')
    
    if [[ "$STATE" == "Completed" ]]; then
        echo "Job completed successfully!"
        RESULTS=$(echo $STATUS | jq -r '.ResultPath')
        echo "Results available at: $RESULTS"
        break
    elif [[ "$STATE" == "Failed" ]]; then
        echo "Job failed!"
        ERROR=$(echo $STATUS | jq -r '.ErrorMessage')
        echo "Error: $ERROR"
        exit 1
    elif [[ "$STATE" == "Running" ]]; then
        PROGRESS=$(echo $STATUS | jq -r '.Progress.PerUn')
        echo "Progress: $(echo "$PROGRESS * 100" | bc)%"
    fi
    
    sleep 30
done
```

## Error Handling

### Common Error Scenarios

1. **Orleans Connection Failed**
   ```
   Error: Unable to connect to Orleans cluster
   ```
   - Check Orleans silo is running
   - Verify connection configuration
   - Ensure network connectivity

2. **Invalid Job ID**
   ```
   Error: Invalid job ID format
   ```
   - Job ID must be a valid GUID
   - Use exact ID from job submission or listing

3. **Job Not Found**
   ```
   Error: Job not found
   ```
   - Job may have been cleaned up
   - Verify job ID is correct
   - Check if job was already deleted

4. **Config File Issues**
   ```
   Error: Config file not found: bot-config.json
   ```
   - Check file path and permissions
   - Ensure JSON format is valid
   - Verify parameter structure matches expected schema

### JSON Error Format

When using `--json`, errors are returned in structured format:

```json
{
  "Error": "Detailed error message describing the issue"
}
```

## Performance Considerations

- Use `--limit` parameter to avoid retrieving large job lists
- For frequent monitoring, consider caching job lists locally
- JSON output is more efficient for programmatic access
- Summary view provides quick cluster overview

## Security Notes

- CLI inherits Orleans security configuration
- Job parameters may contain sensitive trading strategies
- Consider access controls for production environments
- Audit logging tracks CLI job submissions via "CLI" submitted-by field

## Integration Examples

### Monitoring Dashboard

```bash
# Get queue metrics for dashboard
lionfire-trading optimize queue list --summary --json | \
  jq '{queued: .QueuedCount, running: .RunningCount, completed: .CompletedCount}'
```

### Job Submission Pipeline

```bash
# Submit multiple configs
for config in configs/*.json; do
    echo "Submitting $config..."
    lionfire-trading optimize queue add --config-file "$config" --priority 5
done
```

### Health Check

```bash
# Check if any jobs are stuck
STUCK_JOBS=$(lionfire-trading optimize queue list --status running --json | \
  jq '[.[] | select(.Duration and (.Duration | tonumber) > 3600)] | length')

if [ "$STUCK_JOBS" -gt 0 ]; then
    echo "WARNING: $STUCK_JOBS jobs running longer than 1 hour"
fi
```
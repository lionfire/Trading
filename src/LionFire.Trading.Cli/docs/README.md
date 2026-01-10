# LionFire Trading CLI

Command-line interface for the LionFire Trading platform, providing access to optimization queue management, backtesting operations, and system administration.

## Installation

```bash
# Build the CLI
dotnet build LionFire.Trading.Cli.csproj

# Or publish for deployment
dotnet publish -c Release -o ./publish
```

## Configuration

Configure Orleans connection in `appsettings.json`:

```json
{
  "Orleans": {
    "ClusterId": "trading-cluster",
    "ServiceId": "trading-service",
    "ConnectionString": "localhost"
  }
}
```

## Available Commands

### Optimization Queue Management

Manage distributed optimization jobs across Orleans silos:

```bash
# Submit optimization job
lionfire-trading optimize queue add --config-file bot-config.json

# List jobs in queue
lionfire-trading optimize queue list

# Check job status
lionfire-trading optimize queue status --job-id <guid>

# Cancel job
lionfire-trading optimize queue cancel --job-id <guid>
```

**[📖 Full Queue Commands Documentation →](./Queue-Commands.md)**

### Backtesting Operations

Run individual backtests and optimization tasks:

```bash
# Run single backtest
lionfire-trading backtest --config-file backtest-config.json

# Run parameter optimization
lionfire-trading optimize --strategy GridSearch --config-file config.json
```

## Command Structure

All commands follow the pattern:
```
lionfire-trading <area> <command> [options]
```

**Areas:**
- `optimize` - Optimization queue and parameter optimization
- `backtest` - Individual backtesting operations
- `system` - System administration and diagnostics

**Global Options:**
- `--json` - Output results as JSON for scripting
- `--verbose` - Enable detailed logging
- `--help` - Show command help

## Output Formats

### Human-Readable Tables
```
┌─────────────┬───────────┬──────────┬─────────────┐
│ Job ID      │ Status    │ Priority │ Created     │
├─────────────┼───────────┼──────────┼─────────────┤
│ a1b2c3d4... │ Running   │ 3        │ 2 hrs ago   │
│ b2c3d4e5... │ Queued    │ 5        │ 1 hr ago    │
└─────────────┴───────────┴──────────┴─────────────┘
```

### JSON for Automation
```json
{
  "JobId": "a1b2c3d4-5e6f-7890-abcd-ef1234567890",
  "Status": "Running",
  "Priority": 3,
  "Progress": {
    "PerUn": 0.673,
    "Completed": 1346,
    "Total": 2000
  }
}
```

## Integration Examples

### Bash Automation
```bash
#!/bin/bash
# Submit and monitor optimization job
JOB_ID=$(lionfire-trading optimize queue add --config-file config.json --json | jq -r '.JobId')
echo "Monitoring job: $JOB_ID"

while true; do
    STATUS=$(lionfire-trading optimize queue status --job-id $JOB_ID --json)
    STATE=$(echo $STATUS | jq -r '.Status')
    
    case $STATE in
        "Completed") echo "Job completed!"; break ;;
        "Failed") echo "Job failed!"; exit 1 ;;
        "Running") echo "Progress: $(echo $STATUS | jq -r '.Progress.PerUn * 100')%" ;;
    esac
    
    sleep 30
done
```

### PowerShell Integration
```powershell
# Submit multiple configurations
Get-ChildItem "configs/*.json" | ForEach-Object {
    Write-Host "Submitting $($_.Name)..."
    $result = lionfire-trading optimize queue add --config-file $_.FullName --json | ConvertFrom-Json
    Write-Host "Job ID: $($result.JobId)"
}
```

### CI/CD Pipeline
```yaml
# GitHub Actions example
- name: Submit optimization job
  run: |
    JOB_ID=$(lionfire-trading optimize queue add --config-file .github/configs/release.json --json | jq -r '.JobId')
    echo "JOB_ID=$JOB_ID" >> $GITHUB_ENV

- name: Wait for completion
  run: |
    while true; do
      STATUS=$(lionfire-trading optimize queue status --job-id $JOB_ID --json)
      STATE=$(echo $STATUS | jq -r '.Status')
      if [[ "$STATE" == "Completed" ]]; then break; fi
      if [[ "$STATE" == "Failed" ]]; then exit 1; fi
      sleep 60
    done
```

## Error Handling

Commands return appropriate exit codes:
- `0` - Success
- `1` - General error
- `2` - Invalid arguments
- `3` - Orleans connection failed
- `4` - Job not found

JSON error format:
```json
{
  "Error": "Detailed error message",
  "Code": "ERROR_CODE",
  "Timestamp": "2024-01-15T14:30:00Z"
}
```

## Performance Tips

- Use `--limit` to control output size
- Cache Orleans connection for multiple commands
- Use JSON output for better parsing performance
- Consider batch operations for bulk job management

## Security Considerations

- CLI inherits Orleans cluster security settings
- Job parameters may contain sensitive trading strategies
- Use appropriate access controls in production
- Audit logs track CLI operations via "submitted-by" tracking

## Troubleshooting

### Common Issues

**"Unable to connect to Orleans cluster"**
- Verify Orleans silo is running
- Check connection configuration in appsettings.json
- Ensure network connectivity and firewall settings

**"Invalid job ID format"**
- Job IDs must be valid GUIDs
- Copy exact ID from job submission or listing output

**"Config file not found"**
- Check file path and permissions
- Ensure JSON format is valid
- Verify parameter structure matches expected schema

### Debug Mode

Enable verbose logging:
```bash
lionfire-trading --verbose optimize queue list
```

Check Orleans connectivity:
```bash
lionfire-trading system health --check-orleans
```

## Development

### Adding New Commands

1. Create command class implementing `JasperFxAsyncCommand<TInput>`
2. Add input class with `[Description]` attributes
3. Use `[Area]` attribute for command grouping
4. Support both table and JSON output formats

### Testing

```bash
# Unit tests
dotnet test ../tests/LionFire.Trading.Cli.Tests/

# Integration tests (requires Orleans silo)
dotnet test ../tests/LionFire.Trading.Cli.Integration.Tests/
```

## Support

- **Documentation**: [Queue Commands](./Queue-Commands.md)
- **Issues**: Report bugs and feature requests via GitHub Issues
- **Architecture**: See `/src/tp/Trading/epics/optimization-queue/PRD.md`
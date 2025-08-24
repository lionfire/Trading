# Optimization Queue System - Clarifying Questions and Answers

## Architecture Decisions

### Q1: Should we use a single global queue grain or partition by some criteria?
**A1**: Use a single global queue grain for simplicity in MVP. The grain can handle the expected load (100+ jobs) efficiently. Future optimization could partition by bot type or exchange if needed.

**Rationale**: Simpler implementation, easier job prioritization across all jobs, Orleans grains can handle this scale efficiently.

### Q2: How should job priorities be determined?
**A2**: Use integer priority where lower numbers = higher priority (0 = highest). Allow users to set priority when queuing jobs.

**Default priorities**:
- 0: Critical/urgent jobs  
- 5: Normal priority (default)
- 10: Low priority/background jobs

### Q3: How should we handle job parameters serialization?
**A3**: Serialize `PMultiSim` objects to JSON using existing serialization infrastructure. Store as string in `OptimizationQueueItem` with metadata for parameter validation.

### Q4: What happens when a silo executing a job crashes?
**A4**: 
- Jobs marked as "Running" will timeout after configurable period (default: 30 minutes)
- Grain will reset job status to "Queued" for retry
- Implement heartbeat mechanism from worker to grain
- Add retry limit (default: 3 attempts) before marking as "Failed"

### Q5: How should we handle job result storage and retrieval?
**A5**: 
- Store results in same location as current OptimizationTask results
- Add `ResultPath` property to `OptimizationQueueItem`
- Results accessible via existing mechanisms (file system paths)
- Queue UI can link to result locations

## UI/UX Decisions

### Q6: Should the Queue button replace the Optimize button or be additional?
**A6**: Keep both buttons. "Optimize" for immediate execution, "Queue" for deferred execution. This gives users choice based on urgency.

### Q7: How detailed should the queue status display be on the main optimize page?
**A7**: Show minimal info on main page:
- "Job #23 in queue (estimated start: 15 min)"
- Link to full queue management page for details

### Q8: Should queued jobs be cancellable from the main optimize page?
**A8**: Yes, add a small "Cancel Queued Job" button that appears when user has a job in queue. Full management available on queue page.

### Q9: How often should the UI poll for status updates?
**A9**: Use Blazor Server's built-in real-time capabilities with 5-second update intervals for queue status. More frequent updates (1-second) when job is running and showing progress.

## CLI Design Decisions

### Q10: Should CLI support all the same parameters as the UI?
**A10**: Start with core parameters for MVP:
- Bot type
- Exchange/Symbol/Timeframe  
- Date range
- Priority
- Configuration file path for complex scenarios

Future iterations can add more parameter support.

### Q11: How should CLI handle authentication/authorization to Orleans cluster?
**A11**: For initial implementation, assume CLI runs in trusted environment. Use Orleans client with connection string. Future iterations can add authentication tokens.

### Q12: Should CLI support interactive mode for job monitoring?
**A12**: No for MVP. Support "fire and forget" queuing with ability to query status. Interactive monitoring can be added later.

## Technical Implementation Questions

### Q13: How should we integrate with existing Orleans hosting in the trading application?
**A13**: 
- Add grain interfaces to shared assembly
- Register grain and hosted service in existing silo startup
- Use existing Orleans configuration and clustering

### Q14: What grain storage provider should be used for queue persistence?
**A14**: Use the same storage provider as existing Orleans grains in the application. Likely Azure Storage or SQL Server depending on current setup.

### Q15: How should we handle versioning of queue item schema?
**A15**: 
- Add version field to `OptimizationQueueItem`
- Use backward-compatible JSON serialization
- Implement migration logic in grain for schema updates

### Q16: Should we support job dependencies or just simple FIFO with priority?
**A16**: Start with simple priority queue for MVP. Job dependencies add significant complexity and may not be needed initially.

### Q17: How should we handle resource constraints (CPU, memory) across silos?
**A17**: 
- Each silo has configurable max concurrent jobs (default: 1 per silo)
- Grain tracks which silos are available for work
- Future enhancement could add resource-aware scheduling

## Security and Permissions

### Q18: Should users only see their own queued jobs or all jobs?
**A18**: For MVP, show all jobs since this is internal trading application. Future versions can add user filtering and permissions.

### Q19: How should we prevent malicious job parameters?
**A19**: 
- Validate parameter ranges and types before queuing
- Use existing parameter validation in `PMultiSim`
- Add resource limits (max date range, parameter counts)

## Error Handling and Monitoring

### Q20: How should we handle and report job failures?
**A20**: 
- Store error details in `OptimizationQueueItem`
- Log failures with detailed context
- UI shows failure reason with retry option
- Consider notification system for critical failures

### Q21: What metrics should we track for queue performance?
**A21**: 
- Queue depth over time
- Average job execution time
- Success/failure rates
- Silo utilization
- User activity (jobs queued per user/day)

### Q22: Should we implement alerts for queue problems?
**A22**: Start with logging and basic metrics. Add alerting in future iterations:
- Queue depth exceeding thresholds
- High failure rates
- Silo unavailability
- Long-running jobs

## Future Enhancement Questions

### Q23: Should we support recurring/scheduled optimization jobs?
**A23**: Not in MVP. This would require additional scheduler component and cron-like configuration. Consider for future releases.

### Q24: Should we support optimization job templates?
**A24**: Not in MVP. Users can save parameter sets using existing mechanisms. Queue system focuses on execution management.

### Q25: Should we support batch job submission?
**A25**: Not in MVP. CLI could support this via scripting. Future UI enhancement could support bulk parameter sweeps.
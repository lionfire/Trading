# Optimization Queue System - Product Requirements Document

## Overview

The Optimization Queue System provides a distributed queuing mechanism for managing optimization jobs across multiple Orleans silos. This system enables users to queue optimization jobs from both the Blazor UI and CLI, with automatic job distribution and execution across the cluster.

## Goals

1. **Distributed Processing**: Enable optimization jobs to be queued and executed across multiple Orleans silos
2. **Priority Management**: Support job prioritization and queue management
3. **UI Integration**: Add queue functionality to existing OneShotOptimize page
4. **CLI Support**: Enable queuing jobs via command line interface
5. **Reliability**: Ensure job persistence and crash recovery

## Architecture

### Orleans Grain Design

The system uses a singleton Orleans grain (`IOptimizationQueueGrain`) to manage the global optimization queue:

- **Grain State**: Persists queue items and metadata to storage
- **Priority Queue**: Uses `PriorityQueue<T>` for job ordering
- **Distributed Access**: Multiple silos can enqueue jobs, single grain manages execution order
- **Worker Distribution**: Each silo runs background service to pull jobs from grain

### Data Models

#### OptimizationQueueItem
- JobId (Guid)
- Priority (int)
- Status (enum: Queued, Running, Completed, Failed, Cancelled)
- Parameters (PMultiSim)
- CreatedTime, StartedTime, CompletedTime
- AssignedSilo (string)
- Progress (OptimizationProgress)
- ResultPath (string)

#### OptimizationJobStatus
- Queued: Job waiting in queue
- Running: Job currently executing
- Completed: Job finished successfully
- Failed: Job failed with error
- Cancelled: Job cancelled by user

### Components

#### 1. Orleans Grain Layer
- `IOptimizationQueueGrain`: Queue management interface
- `OptimizationQueueGrain`: Implementation with state persistence
- `OptimizationQueueState`: Grain state for persistence

#### 2. Background Processing
- `OptimizationQueueProcessor`: Background service in each silo
- Polls grain for available jobs
- Executes optimization tasks
- Reports progress back to grain

#### 3. Blazor UI Updates
- Add "Queue" button to OneShotOptimize toolbar
- Real-time queue status display
- Queue management page at `/optimize/queue`
- Job progress monitoring

#### 4. CLI Extensions
- `optimize queue add` - Add job to queue
- `optimize queue list` - Show queue status
- `optimize queue cancel <id>` - Cancel specific job
- JSON output format for automation

## User Stories

### Story 1: Queue Optimization from UI
**As a** trader using the Blazor UI  
**I want to** queue an optimization job instead of running it immediately  
**So that** I can submit multiple optimization requests without blocking the UI

**Acceptance Criteria:**
- Queue button appears next to Optimize button
- Clicking Queue adds job to global queue
- UI shows queue position and estimated start time
- Can cancel queued jobs from UI

### Story 2: Monitor Queue Status
**As a** trader  
**I want to** see all queued optimization jobs and their status  
**So that** I can track progress and manage my submissions

**Acceptance Criteria:**
- Queue management page shows all jobs
- Real-time status updates
- Can reorder job priorities
- Can cancel specific jobs

### Story 3: CLI Queue Management
**As a** developer/automation script  
**I want to** submit optimization jobs via CLI  
**So that** I can automate optimization workflows

**Acceptance Criteria:**
- CLI commands for queue operations (`add`, `list`, `status`, `cancel`)
- JSON output for scripting and automation
- Can specify job priority and optimization parameters
- Can monitor job progress and real-time status
- Support for config file-based job submission
- Integration with Orleans distributed system

**📖 Implementation:** See [CLI Documentation](/mnt/c/src/Trading/src/LionFire.Trading.Cli/docs/README.md) for complete command reference and examples.

## Technical Requirements

### Performance
- Queue operations must be sub-second
- Support 100+ concurrent jobs in queue
- Efficient job distribution across silos

### Reliability
- Job persistence survives silo restarts
- Automatic job reassignment on silo failure
- Progress tracking and resumption

### Security
- Job isolation between users/tenants
- Secure parameter serialization
- Audit trail for job operations

## Implementation Phases

### Phase 1: Core Infrastructure
- Orleans grain implementation
- Data models and state persistence
- Background job processor

### Phase 2: UI Integration
- OneShotOptimize page updates
- Queue management page
- Real-time status updates

### Phase 3: CLI Support
- Command structure implementation
- Orleans client integration
- Job submission and monitoring

## Dependencies

- Orleans framework for distributed state management
- Existing OptimizationTask and PMultiSim classes
- Blazor Server for real-time UI updates
- LionFire.Trading.Cli project structure

## Risks and Mitigations

### Risk: Grain Hotspotting
**Mitigation**: Use efficient data structures, implement batch operations

### Risk: Job State Consistency
**Mitigation**: Use Orleans transactions, implement proper locking

### Risk: CLI Client Connection
**Mitigation**: Implement connection retry logic, configuration management
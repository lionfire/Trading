using System.Collections.Concurrent;
using LionFire.Trading.Optimization.Matrix;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Manages the priority and enablement state of optimization plan matrices.
/// Caches state in memory and persists changes to the repository.
/// </summary>
public class PlanMatrixService : IPlanMatrixService
{
    private readonly IPlanMatrixStateRepository _repository;
    private readonly ILogger<PlanMatrixService> _logger;
    private readonly ConcurrentDictionary<string, PlanMatrixState> _cache = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public event EventHandler<MatrixStateChangedEventArgs>? StateChanged;

    public PlanMatrixService(
        IPlanMatrixStateRepository repository,
        ILogger<PlanMatrixService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PlanMatrixState> GetStateAsync(string planId)
    {
        if (_cache.TryGetValue(planId, out var cached))
            return cached;

        var state = await _repository.LoadAsync(planId);
        state ??= new PlanMatrixState { PlanId = planId };
        _cache[planId] = state;
        return state;
    }

    // Cell-level operations

    public async Task SetCellPriorityAsync(string planId, string symbol, string timeframe, int priority)
    {
        ValidatePriority(priority);
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var cellKey = PlanMatrixState.CellKey(symbol, timeframe);
            var existing = state.CellStates.GetValueOrDefault(cellKey) ?? new MatrixCellPriority();
            var updated = existing with { ManualPriority = priority };

            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates) { [cellKey] = updated };
            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.CellPriorityChanged, symbol, timeframe);
        }
        finally { semaphore.Release(); }
    }

    public async Task SetCellEnabledAsync(string planId, string symbol, string timeframe, bool enabled)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var cellKey = PlanMatrixState.CellKey(symbol, timeframe);
            var existing = state.CellStates.GetValueOrDefault(cellKey) ?? new MatrixCellPriority();
            var updated = existing with { IsEnabled = enabled };

            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates) { [cellKey] = updated };
            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.CellEnabledChanged, symbol, timeframe);
        }
        finally { semaphore.Release(); }
    }

    public async Task SetCellAutoPriorityAsync(string planId, string symbol, string timeframe, int priority)
    {
        ValidatePriority(priority);
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var cellKey = PlanMatrixState.CellKey(symbol, timeframe);
            var existing = state.CellStates.GetValueOrDefault(cellKey) ?? new MatrixCellPriority();
            var updated = existing with { AutoPriority = priority };

            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates) { [cellKey] = updated };
            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.AutoPriorityChanged, symbol, timeframe);
        }
        finally { semaphore.Release(); }
    }

    public async Task ResetCellToAutopilotAsync(string planId, string symbol, string timeframe)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var cellKey = PlanMatrixState.CellKey(symbol, timeframe);
            if (!state.CellStates.TryGetValue(cellKey, out var existing)) return;

            var updated = existing with { ManualPriority = null };
            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates) { [cellKey] = updated };
            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.CellPriorityChanged, symbol, timeframe);
        }
        finally { semaphore.Release(); }
    }

    // Row-level operations

    public async Task SetRowPriorityAsync(string planId, string symbol, int priority)
    {
        ValidatePriority(priority);
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var existing = state.RowStates.GetValueOrDefault(symbol) ?? new MatrixAxisState();
            var updated = existing with { ManualPriority = priority };

            var newRowStates = new Dictionary<string, MatrixAxisState>(state.RowStates) { [symbol] = updated };
            var newState = state with { RowStates = newRowStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.RowPriorityChanged, symbol);
        }
        finally { semaphore.Release(); }
    }

    public async Task SetRowEnabledAsync(string planId, string symbol, bool enabled)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var existing = state.RowStates.GetValueOrDefault(symbol) ?? new MatrixAxisState();
            var updated = existing with { IsEnabled = enabled };

            var newRowStates = new Dictionary<string, MatrixAxisState>(state.RowStates) { [symbol] = updated };
            var newState = state with { RowStates = newRowStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.RowEnabledChanged, symbol);
        }
        finally { semaphore.Release(); }
    }

    public async Task DisableRowAsync(string planId, string symbol)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates);

            foreach (var kvp in state.CellStates)
            {
                if (kvp.Key.StartsWith(symbol + "|", StringComparison.Ordinal))
                {
                    newCellStates[kvp.Key] = kvp.Value with { IsEnabled = false };
                }
            }

            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.BulkChange, symbol);
        }
        finally { semaphore.Release(); }
    }

    // Column-level operations

    public async Task SetColumnPriorityAsync(string planId, string timeframe, int priority)
    {
        ValidatePriority(priority);
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var existing = state.ColumnStates.GetValueOrDefault(timeframe) ?? new MatrixAxisState();
            var updated = existing with { ManualPriority = priority };

            var newColStates = new Dictionary<string, MatrixAxisState>(state.ColumnStates) { [timeframe] = updated };
            var newState = state with { ColumnStates = newColStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.ColumnPriorityChanged, timeframe: timeframe);
        }
        finally { semaphore.Release(); }
    }

    public async Task SetColumnEnabledAsync(string planId, string timeframe, bool enabled)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var existing = state.ColumnStates.GetValueOrDefault(timeframe) ?? new MatrixAxisState();
            var updated = existing with { IsEnabled = enabled };

            var newColStates = new Dictionary<string, MatrixAxisState>(state.ColumnStates) { [timeframe] = updated };
            var newState = state with { ColumnStates = newColStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.ColumnEnabledChanged, timeframe: timeframe);
        }
        finally { semaphore.Release(); }
    }

    public async Task DisableColumnAsync(string planId, string timeframe)
    {
        var semaphore = GetLock(planId);
        await semaphore.WaitAsync();
        try
        {
            var state = await GetStateAsync(planId);
            var newCellStates = new Dictionary<string, MatrixCellPriority>(state.CellStates);

            var suffix = "|" + timeframe;
            foreach (var kvp in state.CellStates)
            {
                if (kvp.Key.EndsWith(suffix, StringComparison.Ordinal))
                {
                    newCellStates[kvp.Key] = kvp.Value with { IsEnabled = false };
                }
            }

            var newState = state with { CellStates = newCellStates };
            await SaveAndCacheAsync(newState);

            OnStateChanged(planId, MatrixStateChangeType.BulkChange, timeframe: timeframe);
        }
        finally { semaphore.Release(); }
    }

    // Internal helpers

    private async Task SaveAndCacheAsync(PlanMatrixState state)
    {
        _cache[state.PlanId] = state;
        await _repository.SaveAsync(state);
    }

    private SemaphoreSlim GetLock(string planId) =>
        _locks.GetOrAdd(planId, _ => new SemaphoreSlim(1, 1));

    private static void ValidatePriority(int priority)
    {
        if (priority < 1 || priority > 9)
            throw new ArgumentOutOfRangeException(nameof(priority), priority, "Priority must be between 1 and 9.");
    }

    private void OnStateChanged(string planId, MatrixStateChangeType changeType, string? symbol = null, string? timeframe = null)
    {
        StateChanged?.Invoke(this, new MatrixStateChangedEventArgs(planId, changeType, symbol, timeframe));
    }
}

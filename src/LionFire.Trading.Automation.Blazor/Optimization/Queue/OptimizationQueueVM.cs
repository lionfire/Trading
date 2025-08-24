using LionFire.Blazor.Components;
using LionFire.Mvvm;
using LionFire.ReactiveUI_;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using LionFire.Trading.Grains.Optimization;
using LionFire.Trading.Optimization.Queue;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace LionFire.Trading.Automation.Blazor.Optimization.Queue;

public partial class OptimizationQueueVM : ReactiveObject, IDisposable
{
    #region Dependencies

    public IGrainFactory GrainFactory { get; }
    public ILogger<OptimizationQueueVM> Logger { get; }

    #endregion

    #region State

    [Reactive]
    private ObservableCollection<OptimizationQueueItem> _jobs = new();

    [Reactive]
    private OptimizationQueueStatus? _queueStatus;

    [Reactive]
    private bool _isLoading;

    [Reactive]
    private string _errorMessage = string.Empty;

    [Reactive]
    private OptimizationJobStatus? _statusFilter;

    [Reactive]
    private bool _autoRefresh = true;

    private Timer? _refreshTimer;

    #endregion

    #region Lifecycle

    public OptimizationQueueVM(IGrainFactory grainFactory, ILogger<OptimizationQueueVM> logger)
    {
        GrainFactory = grainFactory;
        Logger = logger;

        // Auto-refresh every 5 seconds when enabled
        this.WhenAnyValue(x => x.AutoRefresh).Subscribe(enabled =>
        {
            if (enabled)
            {
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        });

        // Initial load
        _ = Task.Run(RefreshAsync);
    }

    public void Dispose()
    {
        StopAutoRefresh();
    }

    #endregion

    #region Methods

    public async Task RefreshAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");

            // Get queue status and jobs
            var statusTask = queueGrain.GetQueueStatusAsync();
            var jobsTask = queueGrain.GetJobsAsync(StatusFilter, 200);

            await Task.WhenAll(statusTask, jobsTask);

            QueueStatus = await statusTask;
            var jobsList = await jobsTask;

            // Update jobs collection
            Jobs.Clear();
            foreach (var job in jobsList.OrderByDescending(j => j.CreatedTime))
            {
                Jobs.Add(job);
            }

            Logger.LogDebug("Refreshed queue: {JobCount} jobs, {QueuedCount} queued, {RunningCount} running",
                Jobs.Count, QueueStatus.QueuedCount, QueueStatus.RunningCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to refresh queue data");
            ErrorMessage = $"Failed to refresh: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> CancelJobAsync(Guid jobId)
    {
        try
        {
            var queueGrain = GrainFactory.GetGrain<IOptimizationQueueGrain>("global");
            var result = await queueGrain.CancelJobAsync(jobId);

            if (result)
            {
                Logger.LogInformation("Successfully cancelled job {JobId}", jobId);
                await RefreshAsync();
            }
            else
            {
                Logger.LogWarning("Failed to cancel job {JobId}", jobId);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cancelling job {JobId}", jobId);
            ErrorMessage = $"Failed to cancel job: {ex.Message}";
            return false;
        }
    }

    public async Task SetStatusFilterAsync(OptimizationJobStatus? status)
    {
        StatusFilter = status;
        await RefreshAsync();
    }

    public string GetStatusDisplayText(OptimizationJobStatus status)
    {
        return status switch
        {
            OptimizationJobStatus.Queued => "Queued",
            OptimizationJobStatus.Running => "Running",
            OptimizationJobStatus.Completed => "Completed",
            OptimizationJobStatus.Failed => "Failed",
            OptimizationJobStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    public MudBlazor.Color GetStatusColor(OptimizationJobStatus status)
    {
        return status switch
        {
            OptimizationJobStatus.Queued => MudBlazor.Color.Default,
            OptimizationJobStatus.Running => MudBlazor.Color.Primary,
            OptimizationJobStatus.Completed => MudBlazor.Color.Success,
            OptimizationJobStatus.Failed => MudBlazor.Color.Error,
            OptimizationJobStatus.Cancelled => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Default
        };
    }

    public string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue) return "-";

        var d = duration.Value;
        if (d.TotalDays >= 1)
            return $"{d.Days}d {d.Hours}h {d.Minutes}m";
        if (d.TotalHours >= 1)
            return $"{d.Hours}h {d.Minutes}m";
        if (d.TotalMinutes >= 1)
            return $"{d.Minutes}m {d.Seconds}s";
        
        return $"{d.Seconds}s";
    }

    public string FormatEstimatedCompletion(OptimizationQueueItem job)
    {
        if (job.Status != OptimizationJobStatus.Running)
            return "-";

        var estimated = job.EstimatedCompletionTime;
        if (!estimated.HasValue)
            return "Unknown";

        var remaining = estimated.Value - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero)
            return "Soon";

        return FormatDuration(remaining);
    }

    private void StartAutoRefresh()
    {
        StopAutoRefresh();
        _refreshTimer = new Timer(async _ => await RefreshAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private void StopAutoRefresh()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    #endregion
}
using LionFire.ExtensionMethods;
using LionFire.Trading.Backtesting;

using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public class BacktestBatchItemTask
{
    public IPBacktestTask2 Parameters { get; set; }
    public BacktestResult Result { get; set; }
}

/// <summary>
/// A consumer queue of IBacktestBatchJobs that will execute each job.
/// </summary>
public partial class BacktestQueue
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public ILogger<BacktestQueue> Logger { get; }

    #endregion

    #region Parameters

    public PBacktestBatchQueue Parameters { get; set; } = new();

    #endregion

    #region Lifecycle

    public BacktestQueue(IServiceProvider serviceProvider, IOptionsMonitor<PBacktestBatchQueue> parameters, ILogger<BacktestQueue> logger)
    {
        Parameters = parameters.CurrentValue;
        ServiceProvider = serviceProvider;
        Logger = logger;
        tcs = new();
    }

    #endregion

    #region State

    ConcurrentQueue<BacktestBatchesJob> QueuedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> RunningJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> FinishedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> FaultedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> Jobs { get; } = new();

    TaskCompletionSource tcs;

    #endregion

    #region (Public) Methods

    public IBacktestBatchJob EnqueueJob(Action<IBacktestBatchJob> configure, CancellationToken cancellationToken = default)
    {
        if (Parameters.SingleDateRange != true) throw new NotImplementedException();

        if (RunningJobs.Count >= Parameters.MaxConcurrentJobs && QueuedJobs.Count >= Parameters.MaxQueuedJobs) { throw new InvalidOperationException("Too many active jobs"); }

        var job = Jobs.AddUnique(() => Guid.NewGuid(), guid =>
        {
            var job = new BacktestBatchesJob(guid);
            configure(job);
            return job;
        }).value;

        QueuedJobs.Enqueue(job);

        if (Parameters.AutoStart && RunningJobs.Count < Parameters.MaxConcurrentJobs)
        {
            TryStartNextJobs();
        }

        return job;
    }

    #endregion

    #region (Public) Methods

    /// <summary>
    /// Only need to call this if AutoStart is false.
    /// </summary>
    public void TryStartNextJobs()
    {
        while (QueuedJobs.Count > 0 && RunningJobs.Count < Parameters.MaxConcurrentJobs)
        {
            if (QueuedJobs.TryDequeue(out var job))
            {
                RunningJobs.AddOrThrow(job.Guid, job);
                StartJob(job);
            }
        }
    }

    #endregion

    #region (Private) Methods

    private void StartJob(BacktestBatchesJob job)
    {
        Task.Run(async () =>
        {
            try
            {
                var sw = Stopwatch.StartNew();
                int count = 0;
                foreach (var batch in job.BacktestBatches)
                {
                    var batchBacktest = await BacktestBatchTask2<double>.Create(ServiceProvider, batch, backtestBatchJournal: job.Journal);
                    await batchBacktest.Run();
                    count++;
                }
                sw.Stop();
                job.OnFinished();
                Logger.LogInformation($"Job {job.Guid} completed {count} batches in {sw.Elapsed}");
            }
            catch(Exception ex)
            {
                job.OnFaulted(ex);
                RunningJobs.Remove(job.Guid, out var _);
                FaultedJobs.AddOrThrow(job.Guid, job);
            }
        }).ContinueWith(t =>
        {

            RunningJobs.Remove(job.Guid, out var _);
            FinishedJobs.AddOrThrow(job.Guid, job);
            TryStartNextJobs();
        });
    }

    #endregion

    AsyncManualResetEvent readyToBatchMore = new AsyncManualResetEvent();

    public bool IsBatchingComplete { get; set; }
}

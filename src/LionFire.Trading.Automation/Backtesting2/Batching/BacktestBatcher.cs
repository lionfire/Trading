using LionFire.ExtensionMethods;
using LionFire.Trading.Backtesting;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public class PBacktestBatcher
{
    public int MaxBatchSize { get; set; } = 100;
    public bool SingleDateRange { get; set; } = true;

    #region Clients

    /// <summary>
    /// Not counting the job that is currently running.  Set to 0 to force one at a time.
    /// </summary>
    public int MaxQueuedJobs { get; set; } = int.MaxValue;
    public int MaxConcurrentJobs => 1;

    #endregion

    #region CPU

    /// <summary>
    /// 0: One per thread
    /// -1: One per thread, 
    /// </summary>
    public int Threads { get; set; } = 0;

    /// <summary>
    /// 0: no max, can use all threads (typically 2 per core)
    /// 1: 1 thread per core
    /// -1: All but one: i.e. 1 thread per core for cores that support 2 threads
    /// </summary>
    public int MaxThreadsPerCore { get; set; } = 0;

    /// <summary>
    /// For CPUs that have both performance and efficiency cores, use performance cores
    /// </summary>
    public bool UsePerformanceCores { get; set; } = true;

    /// <summary>
    /// For CPUs that have both performance and efficiency cores, use efficiency cores
    /// </summary>
    public bool UseEfficiencyCores { get; set; } = true;

    /// <summary>
    /// If null, place no restrictions on which VCores (CPU threads) may be used.
    /// If not null, only these VCores can be used.
    /// </summary>
    public List<int>? VCoreIdWhitelist { get; set; }

    #endregion

    #region Memory

    #endregion

}



public class BacktestBatchItemTask
{
    public IPBacktestTask2 Parameters { get; set; }
    public BacktestResult Result { get; set; }

}

public class BacktestBatchTask
{

}

public interface IBacktestBatchJob
{
    Guid Guid { get; }

    /// <summary>
    /// Enumerable of all Backtests, and an Enumerable of these in case the list is determined as it grows.
    /// </summary>
    IEnumerable<IEnumerable<IPBacktestTask2>> Backtests { get; set; }

    Task Terminated { get; }
}


public class BacktestBatcher
{
    private class BacktestBatchJob : IBacktestBatchJob
    {
        #region Identity

        public Guid Guid { get; }

        #endregion

        #region Lifecycle

        public BacktestBatchJob(Guid guid)
        {
            Guid = guid;
        }

        #endregion

        public Task Task { get; set; }

        public IEnumerable<IEnumerable<IPBacktestTask2>> Backtests { get; set; }

    }

    #region Parameters

    public PBacktestBatcher Parameters { get; set; } = new();

    #endregion

    #region Lifecycle

    public BacktestBatcher(PBacktestBatcher parameters)
    {
        Parameters = parameters;
    }

    #endregion

    public IEnumerable<IPBacktestTask2> ParametersEnumerable { get; set; }

    ConcurrentQueue<BacktestBatchJob> QueuedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchJob> RunningJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchJob> FinishedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchJob> Jobs { get; } = new();

    public IBacktestBatchJob EnqueueJob(Action<IBacktestBatchJob> configure, CancellationToken cancellationToken = default)
    {
        if (Parameters.SingleDateRange != true) throw new NotImplementedException();

        if (RunningJobs.Count >= Parameters.MaxConcurrentJobs && QueuedJobs.Count >= Parameters.MaxQueuedJobs) { throw new InvalidOperationException("Too many active jobs"); }

        var job = Jobs.AddUnique(() => Guid.NewGuid(), guid =>
        {
            var job = new BacktestBatchJob(guid);
            return job;
        }).value;

        QueuedJobs.Enqueue(job);

        if (RunningJobs.Count < Parameters.MaxConcurrentJobs)
        {
            TryStartNextJob();
        }

        return job;

        while (!IsBatchingComplete && !cancellationToken.IsCancellationRequested)
        {
            // TODO: 

            readyToBatchMore.Reset();
            await readyToBatchMore.WaitAsync(cancellationToken);
        }
    }

    private void TryStartNextJob()
    {
        if (RunningJobs.Count >= Parameters.MaxConcurrentJobs) return;

        if(QueuedJobs.TryDequeue(out var job))
        {
            RunningJobs.AddOrThrow(job.Guid, job);
            StartJob(job);
        }
    }

    private void StartJob(BacktestBatchJob job)
    {


    }

    AsyncManualResetEvent readyToBatchMore = new AsyncManualResetEvent();

    public bool IsBatchingComplete { get; set; }
}

using LionFire.ExtensionMethods;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Backtesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;

namespace LionFire.Trading.Automation;

public class BacktestBatchItemTask
{
    public PBacktestTask2 Parameters { get; set; }
    public BacktestResult Result { get; set; }
}

/// <summary>
/// A consumer queue of IBacktestBatchJobs that will execute each job.
/// </summary>
public partial class BacktestQueue : IHostedService
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

        QueuedJobsChannel = Channel.CreateBounded<BacktestBatchesJob>(new BoundedChannelOptions(10_000)
        {
            SingleReader = false,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        runTask = Task.Run(() => Run(cancellationToken));
        return Task.CompletedTask;
    }

    Task runTask;
    private async Task Run(CancellationToken cancellationToken)
    {
        int numThreads = Environment.ProcessorCount;
        var consumers = new Task[numThreads];
        for (int i = 0; i < numThreads; i++)
        {
            consumers[i] = Task.Run(() => Consume());
        }

        await Task.WhenAll(consumers).WaitAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            QueuedJobsChannel.Writer.Complete();
        }
        catch { }

        await runTask;
    }

    #endregion

    #region State

    private Channel<BacktestBatchesJob> QueuedJobsChannel;
    ConcurrentQueue<BacktestBatchesJob> QueuedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> RunningJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> FinishedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> FaultedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestBatchesJob> Jobs { get; } = new();

    TaskCompletionSource tcs;

    #endregion

    #region (Public) Methods

    //public ValueTask<IBacktestBatchJob> EnqueueJob(Action<IBacktestBatchJob> configure, CancellationToken cancellationToken = default)
    //{
    //    var backtestContext = MultiBacktestContext.Create(ServiceProvider, new PMultiBacktestContext());

    //    return EnqueueJob(backtestContext, configure, cancellationToken);
    //}
    public async ValueTask<IBacktestBatchJob> EnqueueJob(MultiBacktestContext backtestContext, Action<IBacktestBatchJob> configure, CancellationToken cancellationToken = default)
    {
        if (Parameters.SingleDateRange != true) throw new NotImplementedException();

        if (RunningJobs.Count >= Parameters.MaxConcurrentJobs && QueuedJobs.Count >= Parameters.MaxQueuedJobs) { throw new InvalidOperationException("Too many active jobs"); }

        var job = Jobs.AddUnique(() => Guid.NewGuid(), guid =>
        {
            var job = new BacktestBatchesJob(guid, backtestContext);
            configure(job);
            return job;
        }).value;

        if (job.Count == 0)
        {
            Debug.WriteLine("Batch has 0 backtests");
        }
        else
        {
            await QueuedJobsChannel.Writer.WriteAsync(job, cancellationToken);

            //QueuedJobs.Enqueue(job); // OLD

            //if (Parameters.AutoStart /*&& RunningJobs.Count < Parameters.MaxConcurrentJobs*/)
            //{
            //    TryStartNextJobs();
            //}
        }

        return job;
    }

    #endregion

    #region (Public) Methods

    async Task Consume()
    {
        try
        {
            while (await QueuedJobsChannel.Reader.WaitToReadAsync())
            {
                while (QueuedJobsChannel.Reader.TryRead(out var job))
                {
                    RunningJobs.AddOrThrow(job.Guid, job);
                    try
                    {
                        await RunJob(job).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Job threw exception");
                        job.OnFaulted(ex);
                    }
                    if (job.CancellationToken.IsCancellationRequested)
                    {
                        Logger.LogDebug("Enqueued Job was canceled: " + job.Guid);
                    }
                    //Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} consumed {item}");
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Only need to call this if AutoStart is false.
    /// </summary>
    public void TryStartNextJobs()
    {
        //while (QueuedJobs.Count > 0 && RunningJobs.Count < Parameters.MaxConcurrentJobs)
        //{
        //    if (QueuedJobs.TryDequeue(out var job))
        //    {
        //        RunningJobs.AddOrThrow(job.Guid, job);
        //        StartJob(job);
        //    }
        //}
    }

    #endregion

    #region (Private) Methods

    private async ValueTask RunJob(BacktestBatchesJob job)
    {
        //Task.Run(async () =>
        //{
        try
        {
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var batch in job.BacktestBatches)
            {
                if (job.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogInformation($"Job {job.Guid} canceled.");
                    break;
                }

                var batchBacktest = await BacktestBatchTask2<double>.Create(ServiceProvider, batch, job.Context, backtestBatchJournal: job.Journal);
                await batchBacktest.Run(job.CancellationToken);
                count++;
            }
            sw.Stop();
            job.OnFinished();
            Logger.LogInformation($"Job {job.Guid} completed {count} batches in {sw.Elapsed} {(job.CancellationToken.IsCancellationRequested ? "(CANCELED)" : "")}");
        }
        catch (Exception ex)
        {
            job.OnFaulted(ex);
            RunningJobs.Remove(job.Guid, out var _);
            FaultedJobs.AddOrThrow(job.Guid, job);
        }
        //}).ContinueWith(t =>
        //{

        RunningJobs.Remove(job.Guid, out var _);
        //FinishedJobs.AddOrThrow(job.Guid, job); // TODO: Keep finished jobs for a short time?  For UI notifications?  They should probably be converted into a lightweight summary for this, and a reference to on-disk details. 
        Jobs.TryRemove(job.Guid, out var _);
        TryStartNextJobs();
        //});
    }

    internal async Task WaitForEmpty()
    {
        throw new NotImplementedException();
    }


    #endregion

    AsyncManualResetEvent readyToBatchMore = new AsyncManualResetEvent();

    public bool IsBatchingComplete { get; set; }
}

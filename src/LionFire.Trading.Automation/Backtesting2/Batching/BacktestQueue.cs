﻿using LionFire.Dependencies;
using LionFire.ExtensionMethods;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Backtesting;
using LionFire.Trading.HistoricalData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using Polly;
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
    public PBotWrapper Parameters { get; set; }
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

        QueuedJobsChannel = Channel.CreateBounded<BacktestJob>(new BoundedChannelOptions(10_000)
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

    private Channel<BacktestJob> QueuedJobsChannel;
    ConcurrentQueue<BacktestJob> QueuedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestJob> RunningJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestJob> FinishedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestJob> FaultedJobs { get; } = new();
    ConcurrentDictionary<Guid, BacktestJob> Jobs { get; } = new();

    TaskCompletionSource tcs;

    #endregion

    #region (Public) Methods

    //public ValueTask<BacktestJob> EnqueueJob(Action<BacktestJob> configure, CancellationToken cancellationToken = default)
    //{
    //    var backtestContext = MultiSimContext.Create(ServiceProvider, new PMultiBacktestContext());

    //    return EnqueueJob(backtestContext, configure, cancellationToken);
    //}

    public async ValueTask<BacktestJob> EnqueueJob(MultiSimContext context, List<PBotWrapper> pBots
        //, Action<BacktestJob>? configure = null
        , CancellationToken cancellationToken = default)
    {
        if (Parameters.SingleDateRange != true) throw new NotImplementedException();

        if (RunningJobs.Count >= Parameters.MaxConcurrentJobs && QueuedJobs.Count >= Parameters.MaxQueuedJobs) { throw new InvalidOperationException("Too many active jobs"); }

        var job = Jobs.AddUnique(() => Guid.NewGuid(),
            guid => new BacktestJob(guid, context, pBots)).value;

        if (job.Count == 0)
        {
            Debug.WriteLine("Batch has 0 backtests");
        }
        else
        {
            await QueuedJobsChannel.Writer.WriteAsync(job, cancellationToken);

            //QueuedJobs.Enqueue(job); // OLD

            //if (PMultiSim.AutoStart /*&& RunningJobs.Count < PMultiSim.MaxConcurrentJobs*/)
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
                        job.MultiSimContext.OnFaulted(ex);
                    }
                    if (job.MultiSimContext.CancellationToken.IsCancellationRequested)
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
        //while (QueuedJobs.Count > 0 && RunningJobs.Count < PMultiSim.MaxConcurrentJobs)
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

    private ValueTask RunJob(BacktestJob job) => RunJob<double>(job); // HARDCODE TPrecision for now

    private async ValueTask RunJob<TPrecision>(BacktestJob job)
        where TPrecision : struct, INumber<TPrecision>
    {
        try
        {
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var batch in job.BacktestBatches)
            {
                if (job.MultiSimContext.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Job {guid} canceled.", job.Guid);
                    break;
                }

                var pBatch = new PBatch(job.MultiSimContext, batch)
                {
                };

                var batchContext = ActivatorUtilities.CreateInstance<BatchContext<TPrecision>>(
                   job.MultiSimContext.ServiceProvider,
                   job.MultiSimContext, pBatch);

                var batchHarness = new BatchHarness<TPrecision>(batchContext);
                await batchHarness.Init().ConfigureAwait(false);
                await batchHarness.Run(job.MultiSimContext.CancellationToken);

                count++;
            }
            sw.Stop();
            job.MultiSimContext.OnFinished();
            //Logger.LogInformation($"Job {job.Guid} completed {count} batches in {sw.Elapsed} {(job.MultiSimContext.CancellationToken.IsCancellationRequested ? "(CANCELED)" : "")}"); // OLD
            Logger.LogInformation("Job {guid} completed {count} batches in {milliseconds} Canceled: {canceled}",
                job.Guid, count, sw.Elapsed, job.MultiSimContext.CancellationToken.IsCancellationRequested);
        }
        catch (Exception ex)
        {
            job.MultiSimContext.OnFaulted(ex);
            RunningJobs.Remove(job.Guid, out var _);
            FaultedJobs.AddOrThrow(job.Guid, job);
        }
        //}).ContinueWith(t =>
        //{
        RunningJobs.Remove(job.Guid, out var _);
        //FinishedJobs.AddOrThrow(job.Guid, job); // TODO: Keep finished jobs for a short time?  For UI notifications?  They should probably be converted into a lightweight summary for this, and a reference to on-disk details. 
        Jobs.TryRemove(job.Guid, out var _);
        TryStartNextJobs();
    }

    internal async Task WaitForEmpty()
    {
        throw new NotImplementedException();
    }


    #endregion

    AsyncManualResetEvent readyToBatchMore = new AsyncManualResetEvent();

    public bool IsBatchingComplete { get; set; }
}

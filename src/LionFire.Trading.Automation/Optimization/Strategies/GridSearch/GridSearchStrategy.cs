
using DynamicData;
using Hjson;
using LionFire.ExtensionMethods.Collections;
using LionFire.ExtensionMethods.Copying;
using LionFire.Extensions.Logging;
using LionFire.Serialization.Csv;
using LionFire.Threading;
using LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;
using LionFire.Trading.Optimization;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using NLog.LayoutRenderers.Wrappers;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Channels;

namespace LionFire.Trading.Automation.Optimization.Strategies;

public class GridSearchStrategy : OptimizationStrategyBase, IOptimizationStrategy
{
    #region Parent

    public OptimizationTask OptimizationTask { get; }

    public MultiSimContext MultiSimContext => OptimizationTask.MultiSimContext;

    internal ILogger Logger { get; }

    #region Derived

    public IServiceProvider ServiceProvider => OptimizationTask.ServiceProvider;
    private BacktestsJournal BacktestsJournal => OptimizationTask.Journal!;

    #endregion

    #endregion

    #region Parameters

    public PGridSearchStrategy Parameters { get; set; }

    #endregion

    #region Lifecycle

    public GridSearchStrategy(ILogger<GridSearchStrategy> logger, POptimization optimizationParameters, OptimizationTask optimizationTask) : base(optimizationTask, optimizationParameters)
    {
        Parameters = optimizationParameters.POptimizationStrategy as PGridSearchStrategy ?? throw new ArgumentException("optimizationParameters.POptimizationStrategy must be PGridSearchStrategy");

        OptimizationTask = optimizationTask ?? throw new ArgumentNullException();
        Logger = logger; // optimizationTask.ServiceProvider.GetRequiredService<ILogger<GridSearchStrategy>>();
        State = new GridSearchState(this);

        if (optimizationTask.MultiSimContext.OutputDirectory != null)
        {
            foreach (var level in State.LevelsOfDetail)
            {
                var json = JsonSerializer.Serialize(
                    level,
                    //level.PMultiSim.OfType<object>(),
                    new JsonSerializerOptions
                    {
                        WriteIndented = false,

                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
                    });
                var hjsonValue = Hjson.JsonValue.Parse(json);
                var hjson = hjsonValue.ToString(new HjsonOptions { EmitRootBraces = false });

                var path = Path.Combine(optimizationTask.MultiSimContext.OutputDirectory, $"GridLevel {level.Level}.hjson");
                File.WriteAllText(path, hjson);
            }
        }
    }

    #endregion

    #region State

    public GridSearchState State { get; private set; }

    public long BacktestsComplete => MultiSimContext.BatchEvents.Completed;
    public long BacktestsQueued { get; set; }

    public long MinBacktestsRemaining { get; set; }
    public long MaxBacktestsRemaining { get; set; }
    public double MinPercentComplete => BacktestsComplete == 0 ? 0
        : ((double)BacktestsComplete / (double)MinBacktestsRemaining + (double)BacktestsComplete);
    public double MaxPercentComplete => BacktestsComplete == 0 ? 0
        : ((double)BacktestsComplete / (double)MaxBacktestsRemaining + (double)BacktestsComplete);

    public DateTimeOffset? StartTime { get; set; }
    public TimeSpan PausedTime { get; set; }
    public bool IsPaused { get; set; }

    #region Derived

    public OptimizationProgress Progress
    {
        get
        {
            if (progress == null)
            {
                if (!StartTime.HasValue)
                {
                    return NoProgress;
                }
                else
                {
                    progress = new OptimizationProgress
                    {
                        Start = StartTime,
                    };
                }
            }

            if (State.LevelsOfDetail != null)
            {
                progress.Completed = BacktestsComplete;
                progress.FractionallyCompleted = MultiSimContext.BatchEvents.FractionallyCompleted;
                progress.Queued = BacktestsQueued;
                //progress.Total = (long)State.LevelsOfDetail.Select(l => l.TestPermutationCount).Sum();
                //progress.PlannedSearchTotal = ; // FUTURE: Everything at level 1+
                progress.PauseElapsed = PausedTime;
                progress.IsPaused = IsPaused;
            }

            return progress;
        }
    }
    private OptimizationProgress progress = new();
    static OptimizationProgress NoProgress { get; } = new();

    #endregion

    #endregion

    #region Logic

    //public class ParameterOrderComparer : IComparer<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)>
    //{
    //    public int Compare((HierarchicalPropertyInfo info, IParameterOptimizationOptions options) x, (HierarchicalPropertyInfo info, IParameterOptimizationOptions options) y)
    //    {
    //        if (y.)
    //        {
    //        }
    //        return x.info.Period
    //    }
    //}

    public CancellationToken CancellationToken => MultiSimContext.CancellationToken;

    public async Task Run()
    {
        Logger.LogInformation("Starting: {0}", OptimizationParameters);
        //Logger.LogInformation($"{nameof(OptimizationParameters)}");

        StartTime = DateTimeOffset.UtcNow;

        var optimizableParameters = State.OptimizableParameters;
        var unoptimizableParameters = State.UnoptimizableParameters;

        // Start a loop to read from the ParametersToTest channel
        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();
        int maxBatchSize = OptimizationParameters.MaxBatchSize;

        var ParametersToTest = Channel.CreateBounded<List<int>>(new BoundedChannelOptions(maxBatchSize * 2)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
            AllowSynchronousContinuations = true
            //SingleReader = false, SingleWriter = false, AllowSynchronousContinuations = false
        });

        bool consumerFinished = false;
        int backtestsEnqueued = 0;
        AutoResetEvent backtestFinishedEvent = new(false);
        void OnBacktestFinished() { Interlocked.Decrement(ref backtestsEnqueued); if (backtestsEnqueued == 0) { backtestFinishedEvent.Set(); } }

        var enqueueTask = Task.Run(async () =>
        {
            List<int[]> batchStaging = new(OptimizationTask.Parameters.MaxBatchSize);
            while (!CancellationToken.IsCancellationRequested
            && !ParametersToTest.Reader.Completion.IsCompleted)
            {
                batchStaging.Clear();

                int zeroesCount = 0;
                await foreach (var parameters /*Bundle */in ParametersToTest.Reader.ReadAllAsync(CancellationToken)
                .ConfigureAwait(false)
                )
                {
                    //foreach (var parameters in parametersBundle)
                    {
                        //if (!parameters.Where(p => p != 0).Any())
                        //{
                        //    if (zeroesCount++ > 1)
                        //    {
                        //        Debug.WriteLine("All zeros count: " + zeroesCount);
                        //    }
                        //}
                        if (CancellationToken.IsCancellationRequested) { Debug.WriteLine("CancellationToken is canceled"); }
                        //Debug.WriteLine("R: " + parameters.Select(p => p.ToString()).Aggregate((x, y) => $"{x}, {y}"));
                        batchStaging.Add(parameters.ToArray());
                        if (batchStaging.Count >= OptimizationTask.Parameters.MaxBatchSize)
                        {
                            break;
                        }
                    }
                }

                var pBotType = MultiSimContext.Parameters.PBotType ?? throw new ArgumentNullException("MultiSimContext.Parameters.PBotType");
                
                OptimizationParameters.ValidateOrThrow();

                if (batchStaging.Count > 0)
                {
                    int batchStagingIndex = 0;

                    while (batchStagingIndex < batchStaging.Count && !CancellationToken.IsCancellationRequested)
                    {
                        var enqueueJobSW = Stopwatch.StartNew();

                        List<PBotWrapper> backtestTasksBatch = new(maxBatchSize);
                        {
                            //Logger.LogInformation("Enqueuing batch {0} with {1} items", batchStagingIndex, batchStaging.Count);

                            //backtestTasksBatch.Clear();

                            for (
                                    ; batchStagingIndex < batchStaging.Count
                                        && backtestTasksBatch.Count < maxBatchSize
                                    ; batchStagingIndex++)
                            {
                                var pBot = Activator.CreateInstance(pBotType);

                                foreach (var poo in unoptimizableParameters)
                                {
                                    poo.Info.SetValue(pBot, poo.SingleValue ?? poo.DefaultValue);
                                }

                                int parameterIndex = 0;
                                foreach (var kvp in optimizableParameters)
                                {
                                    kvp.Info.SetValue(pBot, State.CurrentLevel.Parameters[parameterIndex]
                                        .GetValue(batchStaging[batchStagingIndex][parameterIndex]));
                                    parameterIndex++;
                                }

                                //foreach (var (src, dest) in propertiesToCopy) { dest.SetValue(pBot, src.GetValue(pBacktest)); }
                                //foreach (var range in segments) { range.Info.SetValue(pBot, range.CurrentValue); }

                                var pItem = new PBotWrapper //(OptimizationParameters.CommonBacktestParameters)
                                {
                                    PBot = (IPTimeFrameBot2)pBot!,
                                    OnFinished = OnBacktestFinished
                                };
                                backtestTasksBatch.Add(pItem);
                                Interlocked.Increment(ref backtestsEnqueued);

                                //foreach (var range in ((IEnumerable<IParameterValuesSegment>)segments).Reverse())
                                //{
                                //    if (!range.IsFinished)
                                //    {
                                //        range.MoveNext();
                                //        break;
                                //    }
                                //}
                            }
                        }

                        var job = await batchQueue.EnqueueJob(MultiSimContext, backtestTasksBatch
                        //    , job =>
                        //{
                        //    job.Backtests = backtestTasksBatch;
                        //}
                        , CancellationToken);

                        Logger.Log(enqueueJobSW.ElapsedMilliseconds > 50 ? LogLevel.Information : LogLevel.Trace, "Enqueued batch #{0} {1} with {2} items in {3}ms", batchStagingIndex, job.Guid, batchStaging.Count, enqueueJobSW.ElapsedMilliseconds);
                        BacktestsQueued += batchStaging.Count;
                    }
                }
                else
                {
                    Logger.LogInformation("BatchStaging is empty.");
                }

                if (batchStaging.Count == 0 && !CancellationToken.IsCancellationRequested)
                {
                    var delay = TimeSpan.FromMilliseconds(1000);
                    Logger.LogInformation($"Batch staging: no parameters available.  Delaying for {delay}ms.");
                    await Task.Delay(delay, CancellationToken);
                    //break;
                }
            }
            Logger.LogInformation("Done enqueuing backtests");
            consumerFinished = true;
            //localCancellationTokenSource.Cancel();
        });

        //HashSet<OptimizationResult> results = new();
        var producerTask = Task.Run(async () =>
        {
            //var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(new[] { cancellationToken, localCancellationTokenSource.Token });

            long remainingBacktestsAllowed = OptimizationParameters.MaxBacktests;

            try
            {
                while (State.CurrentLevelIndex <= 0 && !consumerFinished && remainingBacktestsAllowed > 0 && !CancellationToken.IsCancellationRequested)
                {
                    foreach (var bundle in State.CurrentLevel
                    //.Batch(16)
                    )
                    {
                        //foreach (var current in bundle)
                        {
                            //var bundleCopy = bundle;
                            //var list = bundle.ToList();
                            //var list = new List<int[]>();
                            //list.Add(bundleCopy);
                            var list = new List<int>();
                            list.Add(bundle);


                            //Debug.WriteLine("W:" + bundleCopy.Select(p => p.ToString()).Aggregate((x, y) => $"{x}, {y}"));

                            await ParametersToTest.Writer.WriteAsync(list, CancellationToken)
                            .ConfigureAwait(false)
                            ;
                            //Debug.WriteLine("Write channel parameters: " + copy.Select(p => p.ToString()).Aggregate((x, y) => $"{x}, {y}"));
                            if (remainingBacktestsAllowed-- == 0)
                            {
                                Logger.LogWarning($"Backtest limit reached: {OptimizationParameters.MaxBacktests}");
                                Debug.WriteLine($"Backtest limit reached: {OptimizationParameters.MaxBacktests}");
                                // TODO: Set a warning flag on the results
                                break;
                            }
                        }
                    }
                    break; // TODO NEXT: Go up a level, or optimize promising regions
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    var parameters = new int[optimizableParameters.Count];
            //    for (int i = 0; i < parameters.Length; i++)
            //    {
            //        parameters[i] = 0;
            //    }
            //    await ParametersToTest.Writer.WriteAsync(parameters);
            //}
            //await Task.Yield();
            //await Task.Delay(10000 ); // TEMP
            ParametersToTest.Writer.Complete();
            Logger.LogInformation("Writing to ParametersToTest complete.");
            Debug.WriteLine("Writing to ParametersToTest complete.");
        });

        await producerTask;
        await enqueueTask;

        while (backtestsEnqueued > 0 && !CancellationToken.IsCancellationRequested)
        {
            try
            {
                await backtestFinishedEvent.WaitOneAsync(1000, cancellationToken: CancellationToken);
            }
            catch (OperationCanceledException) { }
        }

        Logger.LogInformation("Done." + (CancellationToken.IsCancellationRequested ? " (Canceled)" : ""));

#if TRIAGE
        //    entry.options.FitnessOfInterest ??= Parameters.FitnessOfInterest; // TRIAGE
        //List<OptimizationLevelOfDetail> levelsOfDetail = new(); // OLD - triage this class: OptimizationLevelOfDetail
        //var pBacktest = OptimizationParameters.CommonBacktestParameters;
        //var propertiesToCopy = new List<(PropertyInfo src, PropertyInfo dest)>(); // TRIAGE - maybe use this as an optimization
#endif

    }


    #endregion
}

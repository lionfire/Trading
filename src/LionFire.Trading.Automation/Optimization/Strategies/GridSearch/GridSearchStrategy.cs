
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

        // Log optimization parameters for debugging
        Logger.LogInformation("Optimization Level {Level}: {Count} parameters to optimize, {TestCount:N0} test permutations",
            State.CurrentLevelIndex, optimizableParameters.Count, State.CurrentLevel.TestPermutationCount);

        foreach (var poo in optimizableParameters)
        {
            var levelParam = State.CurrentLevel.Parameters.FirstOrDefault(p => p.Key == poo.Info.Key);
            Logger.LogInformation("  Parameter {Key}: TestCount={TestCount}, HasStep={HasStep}, Step={Step}, Min={Min}, Max={Max}",
                poo.Info.Key,
                levelParam?.TestCount ?? 0,
                poo.HasStep,
                poo.StepObj,
                poo.MinValueObj,
                poo.MaxValueObj);
        }

        // Start a loop to read from the ParametersToTest channel
        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();
        int maxBatchSize = OptimizationParameters.MaxBatchSize;

        var ParametersToTest = Channel.CreateBounded<int[]>(new BoundedChannelOptions(maxBatchSize * 2)
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
            Logger.LogInformation("Consumer starting. IsCancellationRequested={0}", CancellationToken.IsCancellationRequested);
            int totalItemsRead = 0;

            List<int[]> batchStaging = new(OptimizationTask.Parameters.MaxBatchSize);
            // NOTE: Don't check Completion.IsCompleted in the while condition - it causes a race condition
            // where the producer finishes before the consumer reads. ReadAllAsync handles completion correctly.
            while (!CancellationToken.IsCancellationRequested)
            {
                batchStaging.Clear();

                int zeroesCount = 0;
                Logger.LogDebug("Consumer calling ReadAllAsync. Completion.IsCompleted={0}", ParametersToTest.Reader.Completion.IsCompleted);
                await foreach (var parameters /*Bundle */in ParametersToTest.Reader.ReadAllAsync(CancellationToken)
                .ConfigureAwait(false)
                )
                {
                    //foreach (var parameters in parametersBundle)
                    {
                        totalItemsRead++;
                        Logger.LogDebug("Consumer read item #{0}: [{1}]", totalItemsRead, string.Join(",", parameters));
                        //if (!parameters.Where(p => p != 0).Any())
                        //{
                        //    if (zeroesCount++ > 1)
                        //    {
                        //        Debug.WriteLine("All zeros count: " + zeroesCount);
                        //    }
                        //}
                        if (CancellationToken.IsCancellationRequested) { Debug.WriteLine("CancellationToken is canceled"); }
                        //Debug.WriteLine("R: " + parameters.Select(p => p.ToString()).Aggregate((x, y) => $"{x}, {y}"));
                        batchStaging.Add(parameters);  // parameters is already int[]
                        if (batchStaging.Count >= OptimizationTask.Parameters.MaxBatchSize)
                        {
                            break;
                        }
                    }
                }
                Logger.LogInformation("Consumer ReadAllAsync completed. totalItemsRead={0}, batchStaging.Count={1}", totalItemsRead, batchStaging.Count);

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
                                    var valueIndex = batchStaging[batchStagingIndex][parameterIndex];
                                    var paramValue = State.CurrentLevel.Parameters[parameterIndex].GetValue(valueIndex);

                                    // Debug logging to trace parameter values
                                    Debug.WriteLine($"[GridSearchStrategy] Setting {kvp.Info.Key}: index={valueIndex} -> value={paramValue}");

                                    kvp.Info.SetValue(pBot, paramValue);
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

                        Logger.LogInformation("Enqueued batch #{0} {1} with {2} items in {3}ms", batchStagingIndex, job.Guid, batchStaging.Count, enqueueJobSW.ElapsedMilliseconds);
                        BacktestsQueued += batchStaging.Count;
                    }
                }
                else
                {
                    Logger.LogInformation("BatchStaging is empty.");
                }

                if (batchStaging.Count == 0)
                {
                    if (ParametersToTest.Reader.Completion.IsCompleted)
                    {
                        // Channel is complete and empty - we're done
                        break;
                    }
                    else if (!CancellationToken.IsCancellationRequested)
                    {
                        // Channel is not complete but empty - wait for more items (shouldn't normally happen)
                        var delay = TimeSpan.FromMilliseconds(1000);
                        Logger.LogInformation($"Batch staging: no parameters available.  Delaying for {delay}ms.");
                        await Task.Delay(delay, CancellationToken);
                    }
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
                Logger.LogInformation("Producer starting. CurrentLevelIndex={0}, consumerFinished={1}, remainingBacktestsAllowed={2}, IsCancellationRequested={3}",
                    State.CurrentLevelIndex, consumerFinished, remainingBacktestsAllowed, CancellationToken.IsCancellationRequested);

                int itemsWritten = 0;
                while (State.CurrentLevelIndex <= 0 && !consumerFinished && remainingBacktestsAllowed > 0 && !CancellationToken.IsCancellationRequested)
                {
                    foreach (var bundle in State.CurrentLevel
                    //.Batch(16)
                    )
                    {
                        //foreach (var current in bundle)
                        {
                            itemsWritten++;
                            Logger.LogDebug("Producer writing item #{0}: [{1}]", itemsWritten, string.Join(",", bundle));

                            await ParametersToTest.Writer.WriteAsync(bundle, CancellationToken)
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
                Logger.LogInformation("Producer wrote {0} items to channel", itemsWritten);
            }
            catch (TaskCanceledException) { Logger.LogInformation("Producer caught TaskCanceledException"); }
            catch (OperationCanceledException) { Logger.LogInformation("Producer caught OperationCanceledException"); }

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

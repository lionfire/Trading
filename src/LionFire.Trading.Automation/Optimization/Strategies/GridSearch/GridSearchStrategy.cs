
using DynamicData;
using Hjson;
using LionFire.ExtensionMethods.Copying;
using LionFire.Extensions.Logging;
using LionFire.Serialization.Csv;
using LionFire.Threading;
using LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Channels;

namespace LionFire.Trading.Automation.Optimization.Strategies;

public interface IOptimizationStrategy
{
    int BacktestsComplete { get; }

    int MinBacktestsRemaining { get; }
    int MaxBacktestsRemaining { get; }
}

public class GridSearchStrategy : OptimizationStrategyBase, IOptimizationStrategy
{
    #region Parent

    public OptimizationTask OptimizationTask { get; }

    private ILogger Logger { get; }

    #region Derived

    public IServiceProvider ServiceProvider => OptimizationTask.ServiceProvider;
    private BacktestBatchJournal OptimizationMultiBatchJournal => OptimizationTask.OptimizationMultiBatchJournal!;

    #endregion

    #endregion

    #region Parameters

    public PGridSearchStrategy Parameters { get; set; }
    public POptimization OptimizationParameters { get; }

    #endregion

    #region Lifecycle

    public GridSearchStrategy(PGridSearchStrategy parameters, POptimization optimizationParameters, OptimizationTask optimizationTask) : base(optimizationTask.BacktestContext)
    {
        Parameters = parameters ?? throw new ArgumentNullException();
        OptimizationParameters = optimizationParameters ?? throw new ArgumentNullException();
        OptimizationTask = optimizationTask ?? throw new ArgumentNullException();
        Logger = optimizationTask.ServiceProvider.GetRequiredService<ILogger<GridSearchStrategy>>();
        State = new GridSearchState(this);

        if (optimizationTask.BacktestContext.LogDirectory != null)
        {
            foreach (var level in State.LevelsOfDetail)
            {
                var json = JsonSerializer.Serialize(
                    level,
                    //level.Parameters.OfType<object>(),
                    new JsonSerializerOptions
                {
                    WriteIndented = false,
                    
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
                });
                var hjsonValue = Hjson.JsonValue.Parse(json);
                var hjson = hjsonValue.ToString(new HjsonOptions { EmitRootBraces = false });

                var path = Path.Combine(optimizationTask.BacktestContext.LogDirectory, $"GridLevel {level.Level}.hjson");
                File.WriteAllText(path, hjson);
            }
        }
    }

    #endregion

    #region State

    public GridSearchState State { get; private set; }

    public int BacktestsComplete { get; set; }

    public int MinBacktestsRemaining { get; set; }
    public int MaxBacktestsRemaining { get; set; }
    public double MinPercentComplete => BacktestsComplete == 0 ? 0
        : ((double)BacktestsComplete / (double)MinBacktestsRemaining + (double)BacktestsComplete);
    public double MaxPercentComplete => BacktestsComplete == 0 ? 0
        : ((double)BacktestsComplete / (double)MaxBacktestsRemaining + (double)BacktestsComplete);

    public DateTimeOffset? StartTime { get; set; }
    public TimeSpan PausedTime { get; set; }

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

    public async Task Run(CancellationToken cancellationToken = default)
    {
        StartTime = DateTimeOffset.UtcNow;

        List<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> optimizableParameters = State.optimizableParameters;
        List<(HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> unoptimizableParameters = State.unoptimizableParameters;

        // Start a loop to read from the ParametersToTest channel
        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();
        int maxBatchSize = OptimizationParameters.MaxBatchSize;

        Channel<int[]> ParametersToTest = Channel.CreateBounded<int[]>(new BoundedChannelOptions(maxBatchSize * 2) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true });


        //CancellationTokenSource localCancellationTokenSource = new();

        bool consumerFinished = false;
        int backtestsEnqueued = 0;
        AutoResetEvent backtestFinishedEvent = new(false);
        void OnBacktestFinished() { Interlocked.Decrement(ref backtestsEnqueued); if (backtestsEnqueued == 0) { backtestFinishedEvent.Set(); } }

        var enqueueTask = Task.Run(async () =>
        {
            List<int[]> batchStaging = new(OptimizationTask.Parameters.MaxBatchSize);
            while (!cancellationToken.IsCancellationRequested && !ParametersToTest.Reader.Completion.IsCompleted)
            {
                batchStaging.Clear();

                await foreach (var parameters in ParametersToTest.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    batchStaging.Add(parameters);
                    if (batchStaging.Count >= OptimizationTask.Parameters.MaxBatchSize)
                    {
                        break;
                    }
                }

                if (batchStaging.Count > 0)
                {
                    int batchStagingIndex = 0;

                    while (batchStagingIndex < batchStaging.Count)
                    {
                        var job = await batchQueue.EnqueueJob(BacktestContext, batch =>
                        {
                            batch.Journal = OptimizationMultiBatchJournal;

                            List<PBacktestTask2> backtestTasksBatch = new(maxBatchSize);
                            //backtestTasksBatch.Clear();

                            for (
                                    ; batchStagingIndex < batchStaging.Count
                                        && backtestTasksBatch.Count < maxBatchSize
                                    ; batchStagingIndex++)
                            {
                                var pBot = Activator.CreateInstance(OptimizationParameters.PBotType);

                                foreach (var kvp in unoptimizableParameters)
                                {
                                    kvp.info.SetValue(pBot, kvp.options.DefaultValue);
                                }

                                int parameterIndex = 0;
                                foreach (var kvp in optimizableParameters)
                                {
                                    kvp.info.SetValue(pBot, State.CurrentLevel.Parameters[parameterIndex]
                                        .GetValue(batchStaging[batchStagingIndex][parameterIndex]));
                                    parameterIndex++;
                                }

                                //foreach (var (src, dest) in propertiesToCopy) { dest.SetValue(pBot, src.GetValue(pBacktest)); }
                                //foreach (var range in segments) { range.Info.SetValue(pBot, range.CurrentValue); }

                                var pBacktestTask = new PBacktestTask2(OptimizationParameters.CommonBacktestParameters)
                                {
                                    PBot = (IPTimeFrameBot2)pBot!,
                                    OnFinished = OnBacktestFinished
                                };
                                backtestTasksBatch.Add(pBacktestTask);
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
                            batch.Backtests = backtestTasksBatch;
                        });
                        //await job.Task.ConfigureAwait(false);
                    }
                }

                if (batchStaging.Count == 0)
                {
                    var delay = TimeSpan.FromMilliseconds(1000);
                    Logger.LogInformation($"Batch staging: no parameters available.  Delaying for {delay}ms.");
                    await Task.Delay(delay);
                    //break;
                }
            }
            Logger.LogInformation("Done enqueuing backtests");
            consumerFinished = true;
            //localCancellationTokenSource.Cancel();
        });

        HashSet<OptimizationResult> results = new();
        var producerTask = Task.Run(async () =>
        {
            //var linkedCTS = CancellationTokenSource.CreateLinkedTokenSource(new[] { cancellationToken, localCancellationTokenSource.Token });

            long remainingBacktestsAllowed = OptimizationParameters.MaxBacktests;

            try
            {
                while (State.CurrentLevelIndex <= 0 && !consumerFinished && remainingBacktestsAllowed > 0)
                {
                    foreach (var current in State.CurrentLevel)
                    {
                        await ParametersToTest.Writer.WriteAsync(current, cancellationToken).ConfigureAwait(false);
                        if (remainingBacktestsAllowed-- == 0)
                        {
                            Logger.LogWarning($"Backtest limit reached: {OptimizationParameters.MaxBacktests}");
                            Debug.WriteLine($"Backtest limit reached: {OptimizationParameters.MaxBacktests}");
                            // TODO: Set a warning flag on the results
                            break;
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
            ParametersToTest.Writer.Complete();
        });

        await producerTask;
        await enqueueTask;

        while (backtestsEnqueued > 0)
        {
            await backtestFinishedEvent.WaitOneAsync(1000);
        }


#if TRIAGE
        //    entry.options.FitnessOfInterest ??= Parameters.FitnessOfInterest; // TRIAGE
        //List<OptimizationLevelOfDetail> levelsOfDetail = new(); // OLD - triage this class: OptimizationLevelOfDetail
        //var pBacktest = OptimizationParameters.CommonBacktestParameters;
        //var propertiesToCopy = new List<(PropertyInfo src, PropertyInfo dest)>(); // TRIAGE - maybe use this as an optimization
#endif

    }


    #endregion
}

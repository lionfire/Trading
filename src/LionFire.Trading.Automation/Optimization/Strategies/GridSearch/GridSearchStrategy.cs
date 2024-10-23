
using DynamicData;
using LionFire.ExtensionMethods.Copying;
using LionFire.Extensions.Logging;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Rendering;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;

namespace LionFire.Trading.Automation.Optimization.Strategies;

public interface IOptimizationStrategy
{
    int BacktestsComplete { get; }

    int MinBacktestsRemaining { get; }
    int MaxBacktestsRemaining { get; }
}

public abstract class OptimizationStrategyBase
{

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

    public GridSearchStrategy(PGridSearchStrategy parameters, POptimization optimizationParameters, OptimizationTask optimizationTask)
    {
        Parameters = parameters ?? throw new ArgumentNullException();
        OptimizationParameters = optimizationParameters ?? throw new ArgumentNullException();
        OptimizationTask = optimizationTask ?? throw new ArgumentNullException();
        Logger = optimizationTask.ServiceProvider.GetRequiredService<ILogger<GridSearchStrategy>>();
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

    private IParameterOptimizationOptions GetEffectiveOptions(IParameterOptimizationOptions fromAttribute, IParameterOptimizationOptions? fromOptimizationParameters)
    {
        ArgumentNullException.ThrowIfNull(fromAttribute);

        var clone = fromAttribute.Clone();

        clone.FitnessOfInterest ??= Parameters.FitnessOfInterest;

        if (fromOptimizationParameters != null)
        {
            AssignFromExtensions.AssignNonDefaultPropertiesFrom(fromOptimizationParameters, clone);
        }

        return clone;
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        StartTime = DateTimeOffset.UtcNow;

        SortedDictionary<string, (HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> optimizableParameters = new();
        SortedDictionary<string, (HierarchicalPropertyInfo info, IParameterOptimizationOptions options)> unoptimizableParameters = new();

        foreach (var kvp in BotParameterPropertiesInfo.Get(OptimizationParameters.BotParametersType)
                .PathDictionary
                    .Where(kvp => kvp.Value.IsOptimizable 
                        && kvp.Value.LastPropertyInfo.PropertyType != typeof(bool) // NOTIMPLEMENTED yet
                        )
                    .Select(kvp
                    => new KeyValuePair<string, (HierarchicalPropertyInfo info, IParameterOptimizationOptions options)>(kvp.Key,
                        (info: kvp.Value,
                         options: GetEffectiveOptions(
                                        kvp.Value.ParameterAttribute.GetParameterOptimizationOptions(kvp.Value.LastPropertyInfo!.PropertyType),
                                        Parameters.Parameters.TryGetValue(kvp.Key))))))
        {
            if (kvp.Value.options.IsEligibleForOptimization) { optimizableParameters.Add(kvp.Key, kvp.Value); }
            else { unoptimizableParameters.Add(kvp.Key, kvp.Value); }
        }

        State = new GridSearchState(this, optimizableParameters);

        // Start a loop to read from the ParametersToTest channel
        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();
        int maxBatchSize = OptimizationParameters.MaxBatchSize;

        Channel<int[]> ParametersToTest = Channel.CreateBounded<int[]>(new BoundedChannelOptions(maxBatchSize * 2) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true });

        long remainingBacktestsAllowed = OptimizationParameters.MaxBacktests;

        var enqueueTask = Task.Run(async () =>
        {
            List<int[]> batchStaging = new(OptimizationTask.Parameters.MaxBatchSize);
            while (!cancellationToken.IsCancellationRequested && remainingBacktestsAllowed > 0)
            {
                batchStaging.Clear();

                await foreach (var parameters in ParametersToTest.Reader.ReadAllAsync(cancellationToken))
                {
                    batchStaging.Add(parameters);
                    if (batchStaging.Count >= OptimizationTask.Parameters.MaxBatchSize)
                    {
                        break;
                    }
                }
                int batchStagingIndex = 0;

                List<IPBacktestTask2> backtestTasksBatch = new(maxBatchSize);

                while (batchStagingIndex < batchStaging.Count)
                {
                    var job = batchQueue.EnqueueJob(batch =>
                    {
                        batch.Journal = OptimizationMultiBatchJournal;

                        backtestTasksBatch.Clear();

                        for (
                                ; batchStagingIndex < batchStaging.Count
                                    && backtestTasksBatch.Count < maxBatchSize
                                    && remainingBacktestsAllowed > 0
                                ; batchStagingIndex++
                                    , remainingBacktestsAllowed--)
                        {
                            var pBot = Activator.CreateInstance(OptimizationParameters.BotParametersType);

                            foreach (var kvp in unoptimizableParameters)
                            {
                                kvp.Value.info.SetValue(pBot, kvp.Value.options.SingleValue);
                            }

                            int parameterIndex = 0;
                            foreach (var kvp in optimizableParameters)
                            {
                                kvp.Value.info.SetValue(pBot, State.CurrentLevel.Parameters[parameterIndex]
                                    .GetValue(batchStaging[batchStagingIndex][parameterIndex]));
                                parameterIndex++;
                            }

                            //foreach (var (src, dest) in propertiesToCopy) { dest.SetValue(pBot, src.GetValue(pBacktest)); }
                            //foreach (var range in segments) { range.Info.SetValue(pBot, range.CurrentValue); }

                            var pBacktestTask = new PBacktestTask2(OptimizationParameters.CommonBacktestParameters)
                            {
                                PBot = (IPTimeFrameBot2)pBot!,
                            };
                            backtestTasksBatch.Add(pBacktestTask);

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
                    await job.Task;
                }

                if (batchStaging.Count == 0)
                {
                    var delay = TimeSpan.FromMilliseconds(1000);
                    Logger.LogInformation($"Batch staging: no parameters available.  Delaying for {delay}ms.");
                    break;
                }
            }
        });

        HashSet<OptimizationResult> results = new();
        var producerTask = Task.Run(async () =>
        {
            while (State.CurrentLevelIndex <= 0)
            {
                foreach (var current in State.CurrentLevel)
                {
                    await ParametersToTest.Writer.WriteAsync(current, cancellationToken).ConfigureAwait(false);
                }
            }

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    var parameters = new int[optimizableParameters.Count];
            //    for (int i = 0; i < parameters.Length; i++)
            //    {
            //        parameters[i] = 0;
            //    }
            //    await ParametersToTest.Writer.WriteAsync(parameters);
            //}
            await Task.Yield();
            ParametersToTest.Writer.Complete();
        });

        await producerTask;
        await enqueueTask;

        if (remainingBacktestsAllowed <= 0)
        {
            Logger.LogInformation($"Backtest limit reached: {OptimizationParameters.MaxBacktests}");
            // TODO: Set a warning flag on the results
        }

#if TRIAGE
        //    entry.options.FitnessOfInterest ??= Parameters.FitnessOfInterest; // TRIAGE
        //List<OptimizationLevelOfDetail> levelsOfDetail = new(); // OLD - triage this class: OptimizationLevelOfDetail
        //var pBacktest = OptimizationParameters.CommonBacktestParameters;
        //var propertiesToCopy = new List<(PropertyInfo src, PropertyInfo dest)>(); // TRIAGE - maybe use this as an optimization
#endif


        await Task.Yield(); // TEMP
    }


    #endregion
}

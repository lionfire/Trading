using Hjson;
using LionFire.Applications;
using LionFire.Applications.Trading;
using LionFire.DependencyMachines;
using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Enumerators;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Backtesting2;
using LionFire.Trading.Journal;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Optimization;

public partial class OptimizationTask : ReactiveObject, IRunnable
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    public BacktestQueue? BacktestBatcher { get; private set; }
    public BacktestOptions? BacktestOptions { get; private set; }
    public BacktestExecutionOptions? ExecutionOptions { get; private set; }

    #endregion

    #region Parameters

    public POptimization Parameters => Context.POptimization!;

    public ExchangeSymbol? ExchangeSymbol => Parameters?.CommonBacktestParameters?.ExchangeSymbolTimeFrame;

    #endregion
    PMultiBacktestContext pMultiBacktestContext;
    #region Lifecycle

    public OptimizationTask(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        ServiceProvider = serviceProvider;
        pMultiBacktestContext = parameters ?? throw new ArgumentNullException(nameof(parameters));


    }
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        //IOptimizerEnumerable optimizerEnumerable = Parameters.IsComprehensive ? new ComprehensiveEnumerable(this) : new NonComprehensiveEnumerable(this); // OLD - find this and absorb

        Context = await MultiBacktestContext.Create(ServiceProvider, pMultiBacktestContext);

        if (CancellationTokenSource != null) { throw new AlreadyException(); }
        CancellationTokenSource = new();

        {
            var actualCTS = CancellationTokenSource;
            if (cancellationToken.CanBeCanceled)
            {
                actualCTS = CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token, cancellationToken);
            }
            Context.CancellationToken = actualCTS.Token;
        }

        if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));
        Parameters.ValidateOrThrow();

        //OptimizationDirectory = GetOptimizationDirectory(Parameters.PBotType);

        BacktestBatcher = Parameters.BacktestBatcherName == null ? ServiceProvider.GetRequiredService<BacktestQueue>() : ServiceProvider.GetRequiredKeyedService<BacktestQueue>(Parameters.BacktestBatcherName);

        //if (Parameters.SearchSeed != 0) throw new NotImplementedException(); // NOTIMPLEMENTED

        BacktestOptions = ServiceProvider.GetRequiredService<IOptionsSnapshot<BacktestOptions>>().Value;
        ExecutionOptions = /*executionOptions ?? */ ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestExecutionOptions>>().CurrentValue;

        //await Context.TrySetOptimizationRunInfo(() => getOptimizationRunInfo());


        Context.ValidateOrThrow();

        OptimizationMultiBatchJournal = ActivatorUtilities.CreateInstance<BacktestBatchJournal>(ServiceProvider, Context, Parameters.PBotType!, true);

        var hjson = JsonValue.Parse(JsonSerializer.Serialize(Context.OptimizationRunInfo)).ToString(new HjsonOptions { EmitRootBraces = false });
        string OptimizationRunInfoFileName = "OptimizationRunInfo.hjson";
        await File.WriteAllTextAsync(Path.Combine(Context.OutputDirectory, OptimizationRunInfoFileName), hjson);

        PGridSearchStrategy pGridSearchStrategy = new()
        {
            //Parameters = Parameters.ParameterRanges.ToDictionary(p => p.Name, p => new ParameterOptimizationOptions { MinValue = p.Min, MaxValue = p.Max, MinStep = p.Step, MaxStep = p.Step }),
        };
        Parameters.POptimizationStrategy = pGridSearchStrategy;

        GridSearchStrategy gridSearchStrategy = ActivatorUtilities.CreateInstance<GridSearchStrategy>(ServiceProvider, Parameters, this);
        OptimizationStrategy = gridSearchStrategy;

        RunTask = Task.Run(async () =>
        {
            await gridSearchStrategy.Run().ConfigureAwait(false);
            //await ServiceProvider.GetRequiredService<BacktestQueue>().StopAsync(default);
            if (OptimizationMultiBatchJournal != null)
            {
                await OptimizationMultiBatchJournal.DisposeAsync();
            }
        });
    }

    // TODO: Configure somehow
    public static string MachineName
    {
        get => machineName ?? Environment.MachineName;
        set => machineName = value;
    }
    private static string? machineName;

    #endregion

    #region State

    public IOptimizationStrategy OptimizationStrategy { get; private set; }

    public CancellationToken CancellationToken => CancellationTokenSource?.Token ?? CancellationToken.None;
    public void Cancel() => CancellationTokenSource?.Cancel();

    public MultiBacktestContext Context { get; private set; }

    //string? OptimizationDirectory;
    [Reactive(SetModifier = AccessModifier.Private)]
    private BacktestBatchJournal? _optimizationMultiBatchJournal;

    private IBacktestBatchJob? batchJob = null;

    /// <remarks>
    /// If not null, the task has already been started.
    /// </remarks>
    public CancellationTokenSource? CancellationTokenSource { get; private set; }

    public Task? RunTask { get; private set; }

    #endregion
}


#if SCRAPS
public class InputInfo
{
}

public class BotInfo
{
    public IReadOnlyList<InputInfo> GetInputs(Type botType)
    {
        if (!botType.IsAssignableTo(typeof(IBot2))) throw new ArgumentException($"ValueType {botType} is not an IBot2.");

        var inputSlots = (List<InputSlot>)botType.GetMethod("InputSignals")!.Invoke(null, null)!;


        throw new NotImplementedException();
    }
}


public class OptimizationRunInfo
{
    public static OptimizationRunInfo Get(POptimization optimization)
    {
        //optimization.BotType
        throw new NotImplementedException();
    }

    //public List<ParameterOptimizationInfo> ParameterOptimizationInfos { get; set; }
}
#endif


//public class TOptimizationTask : IHierarchicalTemplate, ITemplate<OptimizationTask>
//{
//    //public List<ITemplate> Children { get; set; }
//    InstantiationCollection Instantiations { get; set; }

//    //IEnumerable<IInstantiation> IHierarchicalTemplate.Children => Children?.OfType<IInstantiation>(); // TODO: Cast/wrap to IInstantiation?  REVIEW the IHierarchicalTemplate interface.
//    IInstantiationCollection IHierarchicalTemplate.Children => Instantiations;
//}
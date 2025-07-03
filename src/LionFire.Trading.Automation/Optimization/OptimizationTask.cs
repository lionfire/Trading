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

/// <summary>
/// Only 1
/// - bot type
/// - Start, EndExclusive
/// - Exchange Area Symbol TimeFrame (default)
/// 
/// 1 or more
/// - batches
/// </summary>
public partial class OptimizationTask : ReactiveObject, IRunnable
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    public BacktestQueue? BacktestBatcher { get; private set; }
    public BacktestOptions? BacktestOptions { get; private set; }
    public BacktestExecutionOptions? ExecutionOptions { get; private set; }

    #endregion

    #region Parameters

    #region (Derived)

    public PMultiSim PMultiSim => MultiSimContext.Parameters;

    public POptimization Parameters => PMultiSim.POptimization!;

    #endregion

    // TODO: Configure somehow
    public static string MachineName
    {
        get => machineName ?? Environment.MachineName;
        set => machineName = value;
    }
    private static string? machineName;

    #endregion

    #region Lifecycle

    public OptimizationTask(IServiceProvider serviceProvider, PMultiSim pMultiSim)
    {
        ServiceProvider = serviceProvider;
        MultiSimContext = ActivatorUtilities.CreateInstance<MultiSimContext>(ServiceProvider, PMultiSim);

        var MachineName = Environment.MachineName; // REVIEW - make configurable

        MultiSimContext.Optimization.OptimizationRunInfo = new()
        {
            Guid = MultiSimContext.Guid.ToString(),
            BotName = BotTyping.TryGetBotType(PMultiSim.PBotType!)?.Name ?? throw new ArgumentNullException("BotName"),
            BotTypeName = MultiSimContext.ServiceProvider.GetRequiredService<BotTypeRegistry>().GetBotNameFromPBot(PMultiSim.PBotType!),

            ExchangeSymbolTimeFrame = MultiSimContext.Parameters.ExchangeSymbolTimeFrame ?? throw new ArgumentNullException(nameof(ExchangeSymbolTimeFrame)),

            Start = MultiSimContext.Parameters.Start,
            EndExclusive = MultiSimContext.Parameters.EndExclusive,

            TicksEnabled = MultiSimContext.Parameters.Features.Ticks(),

            BotAssemblyNameString = PMultiSim.PBotType!.Assembly.FullName ?? throw new ArgumentNullException(nameof(OptimizationRunInfo.BotAssemblyNameString)),
            OptimizationExecutionDate = DateTime.UtcNow,

            MachineName = MachineName,

        };

        MultiSimContext.OptimizationRunInfo.TryHydrateBuildDates(PMultiSim.PBotType!);

        if (Parameters == null) throw new ArgumentNullException(nameof(Parameters));
        Parameters.ValidateOrThrow();
    }


    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await MultiSimContext.Init().ConfigureAwait(false);

        //IOptimizerEnumerable optimizerEnumerable = PMultiSim.IsComprehensive ? new ComprehensiveEnumerable(this) : new NonComprehensiveEnumerable(this); // OLD - find this and absorb

        BacktestBatcher = Parameters.BacktestBatcherName == null ? ServiceProvider.GetRequiredService<BacktestQueue>() : ServiceProvider.GetRequiredKeyedService<BacktestQueue>(Parameters.BacktestBatcherName);

        //if (PMultiSim.SearchSeed != 0) throw new NotImplementedException(); // NOTIMPLEMENTED

        BacktestOptions = ServiceProvider.GetRequiredService<IOptionsSnapshot<BacktestOptions>>().Value;
        ExecutionOptions = /*executionOptions ?? */ ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestExecutionOptions>>().CurrentValue;

        //await MultiSimContext.TrySetOptimizationRunInfo(() => getOptimizationRunInfo());



        var hjson = JsonValue.Parse(JsonSerializer.Serialize(MultiSimContext.OptimizationRunInfo)).ToString(new HjsonOptions { EmitRootBraces = false });
        string OptimizationRunInfoFileName = "OptimizationRunInfo.hjson";
        await File.WriteAllTextAsync(Path.Combine(this.MultiSimContext.OutputDirectory, OptimizationRunInfoFileName), hjson);

        PGridSearchStrategy pGridSearchStrategy = new()
        {
            //PMultiSim = PMultiSim.ParameterRanges.ToDictionary(p => p.Name, p => new ParameterOptimizationOptions { MinValue = p.Min, MaxValue = p.Max, MinStep = p.Step, MaxStep = p.Step }),
        };
        Parameters.POptimizationStrategy = pGridSearchStrategy;

        GridSearchStrategy gridSearchStrategy = ActivatorUtilities.CreateInstance<GridSearchStrategy>(ServiceProvider, Parameters, this);
        OptimizationStrategy = gridSearchStrategy;

        RunTask = Task.Run(async () =>
        {
            await gridSearchStrategy.Run().ConfigureAwait(false);
            //await ServiceProvider.GetRequiredService<BacktestQueue>().StopAsync(default);
            if (Journal != null)
            {
                await Journal.DisposeAsync();
            }
        });
    }

    #endregion

    #region State

    public MultiSimContext MultiSimContext { get; private set; }

    //[Obsolete("REVIEW - use MultiSimContext instead, and this is a Batch context that should be created by whoever creates batches (i.e. GridStrategy or other strategy)")]
    //public BatchContext<double> BatchContext { get; private set; }

    public IOptimizationStrategy OptimizationStrategy { get; private set; }


    public string? OptimizationDirectory => MultiSimContext.OutputDirectory;

    public BacktestsJournal Journal => MultiSimContext.Journal;

    public Task? RunTask { get; private set; }

    #region Cancel State

    #endregion

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
﻿using LionFire.Applications;
using LionFire.Applications.Trading;
using LionFire.DependencyMachines;
using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Optimization.Enumerators;
using LionFire.Trading.Automation.Optimization.Strategies;
using LionFire.Trading.Backtesting2;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationTask : IRunnable
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    public BacktestQueue BacktestBatcher { get; }
    public BacktestOptions BacktestOptions { get; }
    public BacktestExecutionOptions ExecutionOptions { get; }

    #endregion

    #region Parameters

    public POptimization? Parameters => BacktestContext?.OptimizationOptions;

    public ExchangeSymbol? ExchangeSymbol => Parameters?.CommonBacktestParameters?.ExchangeSymbol;

    #endregion

    #region Lifecycle

    public OptimizationTask(IServiceProvider serviceProvider, POptimization parameters)
    {
        ServiceProvider = serviceProvider;
        BacktestContext = MultiBacktestContext.Create(ServiceProvider, new(parameters));
        //OptimizationDirectory = GetOptimizationDirectory(Parameters.PBotType);

        BacktestBatcher = Parameters.BacktestBatcherName == null ? ServiceProvider.GetRequiredService<BacktestQueue>() : ServiceProvider.GetRequiredKeyedService<BacktestQueue>(Parameters.BacktestBatcherName);

        //if (Parameters.SearchSeed != 0) throw new NotImplementedException(); // NOTIMPLEMENTED

        BacktestOptions = ServiceProvider.GetRequiredService<IOptionsSnapshot<BacktestOptions>>().Value;
        ExecutionOptions = /*executionOptions ?? */ ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestExecutionOptions>>().CurrentValue;

    }
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        //IOptimizerEnumerable optimizerEnumerable = Parameters.IsComprehensive ? new ComprehensiveEnumerable(this) : new NonComprehensiveEnumerable(this); // OLD - find this and absorb

        if (CancellationTokenSource != null) { throw new AlreadyException(); }
        CancellationTokenSource = new();

        OptimizationMultiBatchJournal = new BacktestBatchJournal(BacktestContext.LogDirectory, Parameters.PBotType);

        PGridSearchStrategy pGridSearchStrategy = new()
        {
            //Parameters = Parameters.ParameterRanges.ToDictionary(p => p.Name, p => new ParameterOptimizationOptions { MinValue = p.Min, MaxValue = p.Max, MinStep = p.Step, MaxStep = p.Step }),
        };
        GridSearchStrategy gridSearchStrategy = new(pGridSearchStrategy, Parameters, this);

        RunTask = Task.Run(async () =>
        {
            await gridSearchStrategy.Run(CancellationTokenSource.Token);
            //await ServiceProvider.GetRequiredService<BacktestQueue>().StopAsync(default);
            if (OptimizationMultiBatchJournal != null)
            {
                await OptimizationMultiBatchJournal.DisposeAsync();
            }
        });
        return Task.CompletedTask;
    }

    #endregion

    #region State

    public MultiBacktestContext BacktestContext { get; private set; }

    //string? OptimizationDirectory;
    public BacktestBatchJournal? OptimizationMultiBatchJournal { get; private set; }

    private IBacktestBatchJob? batchJob = null;

    /// <remarks>
    /// If not null, the task has already been started.
    /// </remarks>
    public CancellationTokenSource? CancellationTokenSource { get; protected set; }

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
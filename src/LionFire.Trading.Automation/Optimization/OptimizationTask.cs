using LionFire.Applications;
using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.Trading.Automation.Optimization.Enumerators;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Optimization;

//public class TOptimizationTask : IHierarchicalTemplate, ITemplate<OptimizationTask>
//{
//    //public List<ITemplate> Children { get; set; }
//    InstantiationCollection Instantiations { get; set; }

//    //IEnumerable<IInstantiation> IHierarchicalTemplate.Children => Children?.OfType<IInstantiation>(); // TODO: Cast/wrap to IInstantiation?  REVIEW the IHierarchicalTemplate interface.
//    IInstantiationCollection IHierarchicalTemplate.Children => Instantiations;
//}

public class POptimization
{
    /// <summary>
    /// True: Test the entire parameter space at regular intervals (as defined by steps).
    /// False: Do a coarse test of entire parameter space, and then do a fine test of the most promising areas.
    /// </summary>
    public bool IsComprehensive { get; set; }

    /// <summary>
    /// (TODO - NOTIMPLEMENTED) For non-comprehensive tests, this sets the parameters for the initial coarse test.
    /// </summary>
    public int SearchSeed { get; set; }

    /// <summary>
    /// Skip backtests that would alter parameters by a sensitivity amount less than this.  
    /// Set to 0 for an exhaustive test.
    /// </summary>
    /// <remarks>
    /// NOT IMPLEMENTED - how would this actually be calculated? 
    /// </remarks>
    public float SensitivityThreshold { get; set; }

    public required List<PParameterOptimization> Parameters { get; set; }

    public required Type BotParametersType { get; set; }

    /// <summary>
    /// If default, it will use the unkeyed Singleton for BacktestBatchQueue
    /// </summary>
    public object? BacktestBatcherName { get; set; }
}

public class InputInfo
{
}

public class BotInfo
{
    public IReadOnlyList<InputInfo> GetInputs(Type botType)
    {
        if (!botType.IsAssignableTo(typeof(IBot2))) throw new ArgumentException($"Type {botType} is not an IBot2.");

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

public interface PParameterOptimization
{
}
public class PParameterOptimization<T> : PParameterOptimization
    where T : INumber<T>
{
    public T Min { get; set; } = T.Zero;
    public T Max { get; set; } = T.Zero;

    public T Step { get; set; } = T.One;
    public double StepPower { get; set; }
    public OptimizationStepType StepFunctionType { get; set; } = OptimizationStepType.Linear;

}

public enum OptimizationStepType
{
    Linear = 0
}

public class OptimizationTask : IRunnable //: AppTask
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    public BacktestQueue BacktestBatcher { get; }

    #endregion

    #region Parameters

    public POptimization Parameters { get; set; }

    #endregion

    #region Lifecycle

    public OptimizationTask(IServiceProvider serviceProvider, POptimization parameters)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
        BacktestBatcher = Parameters.BacktestBatcherName == null ? ServiceProvider.GetRequiredService<BacktestQueue>() : ServiceProvider.GetRequiredKeyedService<BacktestQueue>(Parameters.BacktestBatcherName);

        if (Parameters.SearchSeed != 0) throw new NotImplementedException();
    }

    IBacktestBatchJob batchJob;
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        IOptimizerEnumerable optimizerEnumerable = Parameters.IsComprehensive ? new ComprehensiveEnumerable(this) : new NonComprehensiveEnumerable(this);

        if(batchJob != null)
        {
            throw new AlreadyException();
        }
        batchJob = BacktestBatcher.EnqueueJob(batchJob =>
        {
            batchJob.BacktestBatches = optimizerEnumerable;
        }, cancellationToken);

        return Task.CompletedTask;
    }

    #endregion

    #region State

    public Task RunTask { get; private set; }

    protected IEnumerable<IPBacktestTask2> CurrentEnumerable { get; private set; }

    #endregion

    //List<> parameterSpaces;
}

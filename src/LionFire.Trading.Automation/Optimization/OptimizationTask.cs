using LionFire.Applications;
using LionFire.Instantiating;
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
    public bool Grid { get; set; }

    /// <summary>
    /// Skip backtests that would alter parameters by a sensitivity amount less than this.  
    /// Set to 0 for an exhaustive test.
    /// </summary>
    public float SensitivityThreshold { get; set; }


    public required List<PParameterOptimization> Parameters { get; set; }

    public required Type BotType { get; set; }

}

public class InputInfo
{

}

public class BotInfo
{
    public IReadOnlyList<InputInfo> GetInputs(Type botType)
    {
        if (!botType.IsAssignableTo(IBot2)) throw new ArgumentException(ArgumentException($"Type {botType} is not an IBot2.");

        var inputSlots = (List<InputSlot>)botType.GetMethod("Inputs")!.Invoke(null, null)!;


    }
}


public class OptimizationRunInfo
{
    public static OptimizationRunInfo Get(POptimization optimization)
    {
        //optimization.BotType
    }

    public List<ParameterOptimizationInfo> ParameterOptimizationInfos { get; set; }
}

public static class POptimizationUtils
{

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

public class OptimizationTask : AppTask
{

    ConcurrentDictionary<int, BacktestHarness> harnesses = new();

    List<> parameterSpaces;

    public Task Start(CancellationToken cancellationToken = default)
    {

    }

}


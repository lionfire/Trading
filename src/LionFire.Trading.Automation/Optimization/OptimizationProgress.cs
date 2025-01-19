namespace LionFire.Trading.Automation.Optimization;

public class OptimizationProgress
{
    public long PlannedScanTotal { get; set; }
    public long ComprehensiveScanTotal { get; set; }
    public double ComprehensiveScanPerUn => ComprehensiveScanTotal == 0 ? 0 : (PlannedScanTotal / ComprehensiveScanTotal);

    public long Skipped { get; set; }
    public long Queued { get; set; }
    public long FractionallyCompleted { get; set; }
    public long Completed { get; set; }
    public double FractionalPercent => FractionalPerUn * 100.0;
    public double Percent => PerUn * 100.0;
    public double PerUn => Queued == 0 ? 0 : (double)Completed / Queued;
    public double FractionalPerUn => Queued == 0 ? 0 : (double)FractionallyCompleted / Queued;
    public long Remaining => Queued - Completed;
    public DateTimeOffset? Start { get; set; }
    public DateTimeOffset? EstimatedEnd { get; set; }
    public TimeSpan? EstimatedDuration => EstimatedEnd - Start;

    public TimeSpan PauseElapsed { get; set; }
    public bool IsPaused { get; set; }

    public static readonly OptimizationProgress NoProgress = new();
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
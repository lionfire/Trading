
namespace LionFire.Trading.Automation.Optimization;

public interface IPParameterOptimization
{
    string Name { get; }
    IParameterValuesSegment Create(HierarchicalPropertyInfo info);
}
public class PParameterOptimization<T> : IPParameterOptimization
    where T : INumber<T>
{
    public string Name { get; set; }

    public T Min { get; set; } = T.Zero;
    public T Max { get; set; } = T.Zero;

    public T Step { get; set; } = T.One;
    public double StepPower { get; set; }
    public OptimizationStepType StepFunctionType { get; set; } = OptimizationStepType.Linear;

    public PParameterOptimization()
    {
        Name = null!;
    }
    public PParameterOptimization(string name)
    {
        Name = name;
    }

    public IParameterValuesSegment Create(HierarchicalPropertyInfo info)
    {
        return new ParameterValuesSegment<T>(this, info);

    }
}

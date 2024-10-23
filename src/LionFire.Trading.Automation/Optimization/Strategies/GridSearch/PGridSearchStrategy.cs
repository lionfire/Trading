namespace LionFire.Trading.Automation.Optimization.Strategies;

public class PGridSearchStrategy
{

    public Dictionary<string, IParameterOptimizationOptions> Parameters { get; set; } = new();
    public double? FitnessOfInterest { get; internal set; }
}

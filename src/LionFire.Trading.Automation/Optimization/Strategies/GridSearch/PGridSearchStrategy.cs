namespace LionFire.Trading.Automation.Optimization.Strategies;

public interface IPOptimizationStrategy
{
    //public Dictionary<string, IParameterOptimizationOptions> Parameters { get; set; } 
}

public class PGridSearchStrategy : IPOptimizationStrategy
{

    //public Dictionary<string, IParameterOptimizationOptions> Parameters { get; set; } = new();

    // REVIEW - I forget what this was for:
    public double? FitnessOfInterest { get; internal set; }
}


namespace LionFire.Trading.Automation.Optimization;

public class OptimizationParameters
{
    public Dictionary<string, BacktestParameter> Parameters { get; set; } = new();
    public static Dictionary<string, BacktestParameter> CommonParameters { get; set; } = new();
    

    static OptimizationParameters()
    {
        CommonParameters.Add("AD", new BacktestParameter
        {
            Name = "AD",
        });
    }
}


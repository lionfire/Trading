#nullable enable
//using LionFire.Trading.Algos.Modular;

namespace LionFire.Trading.Backtesting2;


public class BacktestParameters
{
    public BotParameters? BotParameters { get; set; }

    public string? AlgoType { get; set; }
    public object? AlgoParameters { get; set; } 
}

public class BotParameters
{
    public List<string> LongableSymbols { get; set; } = [];
    public List<string> ShortableSymbols { get; set; } = [];
    public List<string> SymbolsToWatch { get; set; } = [];

}

public class BacktestJob
{
    public BacktestParameters? Parameters { get; set; }

    public object? Algo { get; set; }
    public string? AlgoType => Algo?.GetType().FullName;

}

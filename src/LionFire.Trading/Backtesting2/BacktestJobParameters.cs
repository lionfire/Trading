#nullable enable
using LionFire.Trading.Algos.Modular;

namespace LionFire.Trading.Backtesting2;

public class BacktestParameters
{
    
    public string? AlgoType { get; set; }

    public object AlgoParameters { get; set; }

    public List<string> LongableSymbols { get; set; } = new List<string>();
    public List<string> ShortableSymbols { get; set; } = new List<string>();
    public List<string> SymbolsToWatch { get; set; } = new List<string>();

}

public class BacktestJob
{
    public BacktestParameters Parameters { get; set; }

    public object Algo { get; set; }
    public string? AlgoType => Algo?.GetType().FullName;

}

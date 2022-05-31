#nullable enable

namespace LionFire.Trading.Algos.Parameters
{
    public interface IWatchesSymbols
    {
        List<SymbolIdentifier>? SymbolsToWatch { get; set; } 
    }
}

#nullable enable

namespace LionFire.Trading.Algos.Parameters
{
    public interface ITradesMultipleSymbols
    {
        List<SymbolIdentifier>? Symbols { get; set; } 
    }
}

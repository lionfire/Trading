#nullable enable

namespace LionFire.Trading.Algos.Parameters
{
    public interface ITradesSingleSymbol
    {
        SymbolIdentifier? Symbol { get; set; }
    }
}

namespace LionFire.Trading.Symbols;

public class SymbolIdParserService : ISymbolIdParser
{
    private IEnumerable<ISymbolIdParserStrategy> strategies;

    public SymbolIdParserService(IEnumerable<ISymbolIdParserStrategy> strategies)
    {
        this.strategies = strategies;
    }
    public SymbolIdParseResult? TryParse(string symbol)
    {
        foreach (var s in strategies)
        {
            var result = s.TryParse(symbol);
            if (result != null) return result;
        }

        return null;
    }
}

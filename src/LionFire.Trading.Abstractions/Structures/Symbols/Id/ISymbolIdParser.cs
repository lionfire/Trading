#nullable enable

namespace LionFire.Trading;

public interface ISymbolIdParser
{
    SymbolIdParseResult? TryParse(string symbol);
}
public interface ISymbolIdParserStrategy : ISymbolIdParser { }

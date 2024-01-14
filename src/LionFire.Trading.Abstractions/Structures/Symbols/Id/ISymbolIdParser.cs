#nullable enable

namespace LionFire.Trading;

public interface ISymbolIdParser
{
    SymbolIdParseResult? TryParse(string symbol);
}
public interface IPrioritizedStragegy
{
    /// <summary>
    /// The higher the number, the higher the priority
    /// </summary>
    float Priority { get; }
}
public interface ISymbolIdParserStrategy : ISymbolIdParser { 

}

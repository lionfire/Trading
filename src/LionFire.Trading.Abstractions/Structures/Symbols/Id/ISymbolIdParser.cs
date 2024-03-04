#nullable enable

namespace LionFire.Trading;

public interface ISymbolIdParser
{
    SymbolIdParseResult? TryParse(string symbol);

    string? TryGetRemainder(string key, string separator = " |")
    {
        if (string.IsNullOrEmpty(separator)) throw new ArgumentException(nameof(separator));
        var index = key.IndexOf(separator);
        if (index != -1)
        {
            return key.Substring(index + separator.Length);
        }
        return null;
    }
}

public interface IPrioritizedStragegy
{
    /// <summary>
    /// The higher the number, the higher the priority
    /// </summary>
    float Priority { get; }
}
public interface ISymbolIdParserStrategy : ISymbolIdParser
{

}

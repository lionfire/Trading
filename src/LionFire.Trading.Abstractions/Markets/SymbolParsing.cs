using LionFire.ExtensionMethods;

namespace LionFire.Trading;

// TODO ENH maybe: SymbolParsing service, that is configurable per user (for default base pair, etc.)

public class SymbolParseResult
{
    public bool Success { get; set; }
    public string BaseAsset { get; set; }
    public string QuoteAsset { get; set; }

    public int Scale { get; set; }

}

// ENH: Make more dynamic somehow?  RENAME to CryptoSymbolParsing?
public static class SymbolParsing
{
    public static SymbolParseResult TryParse(string symbol)
    {
        (string first, string second, string? modifier, string? scale) = ParseSymbol(symbol);

        var r = new SymbolParseResult()
        {
            BaseAsset = first,
            QuoteAsset = second,
            Scale = scale != null ? int.Parse(scale) : 1,
            Success = !first.IsNullOrWhiteSpace() && !second.IsNullOrWhiteSpace(),
        };
        return r;
    }

    public static HashSet<string> UsdEquivalents = new HashSet<string>
        {
            "USDT",
            "UST",
            "USDC",
            "TUSD",
            "BUSD",
        };

    public static HashSet<string> SecondPairs = new HashSet<string>
        {
            "BTC",
            "ETH",
            "TRY", // Turkish Lira
            "XRP",
        };
    public static HashSet<string> Scales = new HashSet<string>
        {
            "1000",
        };

    public static (string? first, string? second) TryGetUsdEquivalent(string symbol)
    {
        foreach (var usd in UsdEquivalents)
        {
            if (symbol.EndsWith(usd))
            {
                return (symbol[..^usd.Length], usd);
            }
        }
        return (null, null);
    }
    public static (string? first, string? second) TryGetKnownSecondPair(string symbol)
    {
        foreach (var second in SecondPairs)
        {
            if (symbol.EndsWith(second))
            {
                return (symbol[..^second.Length], second);
            }
        }
        return (null, null);
    }

    public static (string? scale, string? rest) TryGetScale(string symbol)
    {
        foreach (var scale in Scales)
        {
            if (symbol.StartsWith(scale))
            {
                return (symbol[..scale.Length], symbol[scale.Length..]);
            }
        }
        return (null, null);
    }

    public static (string first, string second, string? modifier, string? scale) ParseSymbol(string symbol)
    {
        string? first, second;

        (string? scale, var symbolResult) = TryGetScale(symbol);
        if (symbolResult != null) { symbol = symbolResult; }

        (first, second) = TryGetUsdEquivalent(symbol);
        if (first == null)
        {
            (first, second) = TryGetKnownSecondPair(symbol);
        }

        if (first == null || second == null) throw new ArgumentException("Failed to parse symbol: " + symbol);
        return (first, second, null, scale);
    }

    public static bool IsUsdEquivalent(string symbol)
    {
        var (first, second) = TryGetUsdEquivalent(symbol);
        return second != null;
    }

    public static string ShortSymbol(string symbol)
    {
        return ParseSymbol(symbol).first ?? symbol;
    }
}
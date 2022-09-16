#nullable enable

namespace LionFire.Trading;

public class SymbolIdParser
{
    public static string GetNormalizedAsset(string asset)
    {
        switch (asset)
        {
            case "XBT":
                return "BTC";
            case "XBN":
                return "BCH";
            case "USDC":
            case "BUSD":
            case "USDT":
            case "TUSD":
            case "DAI":
                return "USD";
            default:
                return asset;
        }
    }
    static readonly string[] Denominators = new string[]
    {
        "USDT",
        "BUSD",
        "USD",
        "USDC",
        "EUR",
        "CAD",
        "JPY",
    };
    public static string? TryGetDenominator(string symbol)
    {
        foreach (var d in Denominators)
        {
            if (symbol.EndsWith(d)) { return d; }
        }
        return null;
    }

    public static readonly string[] Separators = new string[] { "/", "-" };

    public static (string?, string?) TryGetNumeratorDenominator(string symbol)
    {
        var denominator = TryGetDenominator(symbol);
        if (denominator == null) return (null, null);

        var numerator = symbol[..^denominator.Length];

        if (numerator.Length > 1 && !char.IsLetterOrDigit(numerator[^1]))
        {
            numerator = numerator.Substring(0, numerator.Length - 1);
        }
        return (numerator, denominator);
    }

    public static string GetNormalizedSymbol(string symbol)
    {
        return symbol switch
        {
            "US SPX 500 (Mini)" => "US500-USD",
            "Europe 50" => "EU50-USD",
            _ => symbol,
        };
    }

    static Dictionary<string, SymbolId> BuiltIn
    {
        get
        {
            if (builtIn == null)
            {
                builtIn = new Dictionary<string, SymbolId>();  // to bootstrap Parse method
                builtIn ??= new Dictionary<string, SymbolId>
                {
                    ["Europe 50"] = Parse("EU50-USD"),
                    ["US SPX 500 (Mini)"] = Parse("US500-USD"),
                };
            }
            return builtIn;
        }
    }
    static Dictionary<string, SymbolId>? builtIn;

    public static SymbolId Parse(string symbol)
    {
        if (BuiltIn.TryGetValue(symbol, out var r)) return r;

        var result = new SymbolId();
        
        symbol = GetNormalizedSymbol(symbol);

        {
            var split = symbol.Split(":");
            if (split.Length > 1)
            {
                result.SetFromExchangeAndAreaCode(split[1]);
            }
            symbol = symbol.Substring(symbol.IndexOf(":") + 1);
        }

        {
            var split = symbol.Split("-");
            if (split.Length == 2)
            {
                result.Numerator = split[0];
                result.Denominator = split[1];
                result.Symbol = result.Numerator + result.Denominator;
                result.NormalizedSymbol = result.Numerator + result.Denominator;
            }
            else if (split.Length == 1)
            {
                (result.Numerator, result.Denominator) = TryGetNumeratorDenominator(symbol);
            }

            if (result.Symbol == null)
            {
                result.Symbol = symbol;
            }

            result.NormalizedNumerator = GetNormalizedAsset(result.Numerator);
            result.NormalizedDenominator = GetNormalizedAsset(result.Denominator);
            result.NormalizedSymbol = GetNormalizedSymbol(result.Symbol);

            return result;
        }
    }

}

using Microsoft.Extensions.Options;

namespace LionFire.Trading;


public class SymbolNameNormalizerOptions
{
    public string Separator = "-";
}

public class SymbolNameNormalizer
{
    SymbolNameNormalizerOptions Options { get; }
    public SymbolNameNormalizer(IOptionsMonitor<SymbolNameNormalizerOptions> optionsMonitor)
    {
        Options = optionsMonitor.CurrentValue;
    }

    public string Normalize(string unnormalizedSymbolName)
    {
        // TODO: detect first and second asset names and put a - or other separator between them
        return unnormalizedSymbolName
            .Replace(".sfl", "")
            .Replace(".spa", "")
            .Replace("USDT", "USD")
            .Replace("BUSD", "USD")
            .Replace("-", "")
            .Replace("US SPX 500 (Mini)", "US500")
            ;
    }
}


namespace LionFire.Trading.Automation.Optimization.Sensitivity;

public enum MarketType
{
    Unknown,
    CryptoMajor,    // BTC, ETH
    CryptoMinor,    // Top 20 alts
    CryptoMeme,     // DOGE, SHIB, PEPE etc.
    Forex,          // EUR/USD, etc.
    Stock,          // AAPL, etc.
}

public static class MarketTypeClassification
{
    private static readonly HashSet<string> CryptoMajors = new(StringComparer.OrdinalIgnoreCase)
    {
        "BTCUSDT", "ETHUSDT", "BTCUSD", "ETHUSD", "BTCBUSD", "ETHBUSD"
    };

    private static readonly HashSet<string> CryptoMinors = new(StringComparer.OrdinalIgnoreCase)
    {
        "BNBUSDT", "SOLUSDT", "XRPUSDT", "ADAUSDT", "DOGEUSDT", "AVAXUSDT",
        "DOTUSDT", "MATICUSDT", "LINKUSDT", "UNIUSDT", "LTCUSDT", "ATOMUSDT",
        "NEARUSDT", "APTUSDT", "OPUSDT", "ARBUSDT", "SUIUSDT", "SEIUSDT"
    };

    private static readonly HashSet<string> CryptoMemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DOGEUSDT", "SHIBUSDT", "PEPEUSDT", "FLOKIUSDT", "BONKUSDT", "WIFUSDT"
    };

    private static readonly string[] CommonForexCurrencies =
    {
        "EUR", "USD", "GBP", "JPY", "AUD", "NZD", "CAD", "CHF"
    };

    /// <summary>
    /// Classify a trading symbol into a market type.
    /// Memes are checked first since some symbols (e.g., DOGEUSDT) appear in both meme and minor sets.
    /// </summary>
    public static MarketType Classify(string symbol)
    {
        if (string.IsNullOrEmpty(symbol)) return MarketType.Unknown;

        // Check memes first (some overlap with minors like DOGE)
        if (CryptoMemes.Contains(symbol)) return MarketType.CryptoMeme;
        if (CryptoMajors.Contains(symbol)) return MarketType.CryptoMajor;
        if (CryptoMinors.Contains(symbol)) return MarketType.CryptoMinor;

        // Pattern-based fallbacks for crypto
        if (symbol.EndsWith("USDT", StringComparison.OrdinalIgnoreCase) ||
            symbol.EndsWith("BUSD", StringComparison.OrdinalIgnoreCase) ||
            symbol.EndsWith("USD", StringComparison.OrdinalIgnoreCase))
            return MarketType.CryptoMinor;

        // Forex patterns (6-char pairs like EURUSD)
        if (symbol.Length == 6 && !symbol.Any(char.IsDigit))
        {
            var prefix = symbol[..3].ToUpperInvariant();
            var suffix = symbol[3..].ToUpperInvariant();
            if (CommonForexCurrencies.Contains(prefix) && CommonForexCurrencies.Contains(suffix))
                return MarketType.Forex;
        }

        return MarketType.Unknown;
    }
}

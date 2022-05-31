#nullable enable

namespace LionFire.Trading
{
    /// <summary>
    /// Matches TradingView style.  e.g. BINANCE:APEUSDTPERP
    /// </summary>
    public struct SymbolIdentifier
    {
        public SymbolIdentifier(string exchange, string symbol)
        {
            Exchange = exchange;
            Symbol = symbol;
        }
        public SymbolIdentifier(string exchangeColonSymbol)
        {
            var split = exchangeColonSymbol.Split(':');
            Exchange = split[0].ToUpperInvariant();
            Symbol = split[1].ToUpperInvariant();
        }

        public string Exchange { get; }
        public string Symbol { get; }

        public static string FromString(string str) => new SymbolIdentifier(str);
        public override string ToString() => $"{Exchange}:{Symbol}";

        public static implicit operator string(SymbolIdentifier symbol) => symbol.ToString();
        public static implicit operator SymbolIdentifier(string symbol) => FromString(symbol);
    }
}

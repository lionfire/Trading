#nullable enable
using System.Text;

namespace LionFire.Trading;

// REVIEW - change set accessors to init?
public class SymbolId : IEquatable<SymbolId>
{
    #region (Static) Configuration

    public static string StandardSplitter { get; set; } = "-";
    public static bool StandardOmitUsd { get; set; } = true;

    #endregion

    #region Construction

    public static SymbolId BinanceFutures(string symbolName)
        => new SymbolId { Exchange = "Binance", ExchangeArea = "Futures", Symbol = symbolName };

    public static SymbolId Parse(string symbolName) => SymbolIdParser.Parse(symbolName);

    #endregion

    public SymbolId() { }
    public SymbolId(string symbol) { Symbol = symbol; }

    public string Exchange { get; set; }
    public string ExchangeArea { get; set; }

    #region Derived

    #region ExchangeAndAreaCode

    public string ExchangeAndAreaCode => $"{Exchange?.Substring(0, 3)}.{ExchangeArea?.Substring(0, 2)}"; // HACK TODO

    public void SetFromExchangeAndAreaCode(string exchangeCode)
    {
        var chunks = exchangeCode.Split('.');
        if (chunks.Length != 2) { throw new ArgumentException("Must be in the format ExchangeAbbreviation.ExchangeAreaAbbreviation"); }

        switch (chunks[0])
        {
            case "Bin":
                Exchange = "Binance";
                break;
            default:
                throw new ArgumentException("Unknown exchange code: " + chunks[0]);
        }
        switch (chunks[1])
        {
            case "Fu":
                ExchangeArea = "Futures";
                break;
            case "Sp":
                ExchangeArea = "Spot";
                break;
            default:
                throw new ArgumentException("Unknown exchange area code: " + chunks[1]);
        }
    }

    #endregion

    public string TradingViewName
    {
        get
        {
            var sb = new StringBuilder();
            switch (Exchange)
            {
                case "Binance":
                    sb.Append("BINANCE");
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Exchange: {Exchange}");
            }
            sb.Append(":");
            sb.Append(Symbol.ToUpperInvariant());

            Exception InvalidExchangeAndAreaCombo() => new ArgumentOutOfRangeException($"Unknown Exchange / ExchangeArea combo: {Exchange} / {ExchangeArea}");

            switch (Exchange)
            {
                case "Binance":
                    switch (ExchangeArea)
                    {
                        case "Futures":
                            sb.Append("PERP");
                            break;
                        case "Spot":
                            break;
                        default:
                            throw InvalidExchangeAndAreaCombo();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Exchange: {Exchange}");
            }

            return sb.ToString();
        }
    }

    #endregion

    public string Symbol { get; set; }
    public string NormalizedSymbol { get; set; }

    public bool HasStandardizedSymbol => Numerator != null && Denominator != null;
    public string StandardizedSymbol
    {
        get
        {
            if (HasStandardizedSymbol)
            {
                return StandardOmitUsd
                    ? $"{NormalizedNumerator ?? Numerator}"
                    : $"{NormalizedNumerator ?? Numerator}{StandardSplitter}{NormalizedDenominator ?? Denominator}";
            }
            return Symbol;
        }
    }
    public string Numerator { get; set; }
    public string NormalizedNumerator { get; set; }
    public string Denominator { get; set; }
    public string NormalizedDenominator { get; set; }

    #region Misc

    public override bool Equals(object? obj) => Equals(obj as SymbolId);

    public bool Equals(SymbolId? other)
        => other is not null &&
               Exchange == other.Exchange &&
               ExchangeArea == other.ExchangeArea &&
               Symbol == other.Symbol;

    public override int GetHashCode() => HashCode.Combine(Exchange, ExchangeArea, Symbol);

    public static bool operator ==(SymbolId left, SymbolId right) => EqualityComparer<SymbolId>.Default.Equals(left, right);

    public static bool operator !=(SymbolId left, SymbolId right) => !(left == right);

    public override string ToString() => $"{Exchange}.{ExchangeArea}.{Symbol}";

    public static SymbolId FromExchangeAndAreaCode(string exchangeAndAreaCode, string symbolName = null)
    {
        var result = new SymbolId { Symbol = symbolName };
        result.SetFromExchangeAndAreaCode(exchangeAndAreaCode);
        return result;
    }

    #endregion


}

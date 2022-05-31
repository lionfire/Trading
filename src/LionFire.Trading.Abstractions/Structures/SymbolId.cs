using System.Text;

namespace LionFire.Trading
{
    public class SymbolId : IEquatable<SymbolId>
    {
        #region Construction

        public static SymbolId BinanceFutures(string symbolName)
            => new SymbolId { Exchange = "Binance", ExchangeArea = "Futures", SymbolName = symbolName };

        #endregion

        public string Exchange { get; set; }
        public string ExchangeArea { get; set; }

        #region Derived

        #region ExchangeAndAreaCode

        public string ExchangeAndAreaCode => $"{Exchange.Substring(0, 3)}.{ExchangeArea.Substring(0, 2)}"; // HACK TODO

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

                sb.Append(SymbolName.ToUpperInvariant());

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

        public string SymbolName { get; set; }

        #region Misc

        public override bool Equals(object obj) => Equals(obj as SymbolId);

        public bool Equals(SymbolId other)
            => other is not null &&
                   Exchange == other.Exchange &&
                   ExchangeArea == other.ExchangeArea &&
                   SymbolName == other.SymbolName;

        public override int GetHashCode() => HashCode.Combine(Exchange, ExchangeArea, SymbolName);

        public static bool operator ==(SymbolId left, SymbolId right) => EqualityComparer<SymbolId>.Default.Equals(left, right);

        public static bool operator !=(SymbolId left, SymbolId right) => !(left == right);

        public override string ToString() => $"{Exchange}.{ExchangeArea}.{SymbolName}";

        public SymbolId FromExchangeAndAreaCode(string exchangeAndAreaCode, string symbolName = null)
        {
            var result = new SymbolId { SymbolName = symbolName };
            result.SetFromExchangeAndAreaCode(exchangeAndAreaCode);
            return result;
        }

        #endregion


    }
}

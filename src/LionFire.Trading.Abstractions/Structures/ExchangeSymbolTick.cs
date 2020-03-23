using System;
using System.Text;

namespace LionFire.Trading
{
    /// <remarks>
    /// Used by: Bitfinex redis feed
    /// </remarks>
    public struct ExchangeSymbolTick
    {
        #region Static Utilities

        //public static string PriceKey(string exchange, string symbol) => $"t:{exchange}:{symbol}";
        //public static string PriceKey(string exchange, string symbol, string type) => $"t:{exchange}:{symbol}:{type}";

        #endregion
        
        public string Key => $"{Exchange}:{Symbol}";
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Spread => Ask - Bid;
        public double Avg => (Bid + Ask) / 2;
        public double Last { get; set; }
        public DateTime Date { get; set; }

        public static ExchangeSymbolTick Parse(string v)
        {
            var valSplit = v.ToString().Split(' ');

            double bid = double.NaN;
            double ask = double.NaN;
            double last = double.NaN;
            var date = default(DateTime);

            foreach (var val in valSplit)
            {
                var kvp = val.Split(':');
                if (kvp[0] == "a")
                {
                    ask = double.Parse(kvp[1]);
                }
                else if (kvp[0] == "b")
                {
                    bid = double.Parse(kvp[1]);
                }
                else if (kvp[0] == "l")
                {
                    last = double.Parse(kvp[1]);
                }
                else if (kvp[0] == "d")
                {
                    date = DateTime.FromBinary(long.Parse(kvp[1]));
                }
            }

            return new ExchangeSymbolTick()
            {
                Ask = ask,
                Bid = bid,
                Last = last,
                Date = date,
            };
        }

        //public static ExchangeSymbolTick FromCsv(string str, char separator = '|')
        //{
        //    var chunks = str.Split(separator);
        //    return new ExchangeSymbolTick()
        //    {
        //        Symbol = chunks[0],
        //        Bid = StringToDouble(chunks[1]),
        //        Ask = StringToDouble(chunks[2]),
        //        Exchange = chunks[3],
        //    };

        //    double StringToDouble(string _str)
        //    {
        //        if (_str == string.Empty) return double.NaN;
        //        return double.Parse(_str);
        //    }
        //}
        
        //public string BidKey => PriceKey(Exchange, Symbol, "bid");
        //public string AskKey => PriceKey(Exchange, Symbol, "ask");

        //public override string ToString() => $"{Symbol}|{Bid}|{Ask}|{Exchange}";
        public override string ToString() {
            var sb = new StringBuilder();

            if (!double.IsNaN(Bid)) sb.Append($" b:{Bid}");
            if (!double.IsNaN(Ask)) sb.Append($" a:{Ask}");
            if (!double.IsNaN(Last)) sb.Append($" l:{Last}");
            if (Date != default(DateTime)) sb.Append($" d:{Date}");

            return sb.ToString();
        }
    }
}

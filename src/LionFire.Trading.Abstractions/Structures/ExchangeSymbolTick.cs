namespace LionFire.Trading
{
    /// <remarks>
    /// Used by: Bitfinex redis feed
    /// </remarks>
    public struct ExchangeSymbolTick
    {
        public string Key => $"{Symbol}@{Exchange}";
        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }

        //public bool IsConnected { get; }

        public static ExchangeSymbolTick FromString(string str)
        {
            var chunks = str.Split('|');
            return new ExchangeSymbolTick()
            {
                Symbol = chunks[0],
                Bid = StringToDouble(chunks[1]),
                Ask = StringToDouble(chunks[2]),
                Exchange = chunks[3],
            };

            double StringToDouble(string _str)
            {
                if (_str == string.Empty) return double.NaN;
                return double.Parse(_str);
            }
        }

        public static string PriceKey(string exchange, string symbol)=> $"t:{symbol}@{exchange}";
        public static string PriceKey(string exchange, string symbol, string type) => $"t:{symbol}@{exchange}:{type}";
        
        public string BidKey => PriceKey(Exchange, Symbol, "bid");
        public string AskKey => PriceKey(Exchange, Symbol, "ask");

        public override string ToString() => $"{Symbol}|{Bid}|{Ask}|{Exchange}";
    }
}

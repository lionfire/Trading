using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    /// <summary>
    /// Facade to bridge cAlgo bots
    /// </summary>
    public class MarketData
    {

        #region Relationships

        public IMarket Market { get; set; }

        #endregion

        #region Config

        public ISingleSeriesConfig Owner { get; set; }


        #endregion

        public Symbol GetSymbol(string symbolCode)
        {
            return Market.GetSymbol(symbolCode);
        }

        public MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame)
        {
            return Market.Data.GetMarketSeries(symbol, timeFrame);
        }

        #region Convenience Accessors

        public MarketSeries GetSeries(TimeFrame timeFrame)
        {
            if (Owner == null) return null;
            return GetSeries(Owner.Symbol, timeFrame);
        }

        public MarketSeries GetSeries(string symbolCode, TimeFrame timeFrame)
        {
            var symbol = GetSymbol(symbolCode);
            if (symbol == null) { throw new ArgumentException("Unknown symbolCode: " + symbolCode); }
            return GetSeries(symbol, timeFrame);
        }

        #endregion

        #region Market Depth

        public MarketDepth GetMarketDepth(string symbolCode) { return new MarketDepth(); }
        public MarketDepth GetMarketDepth(Symbol symbol) { return new MarketDepth(); }

        #endregion
    
    }

}

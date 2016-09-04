using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public class LiveDataSource : IDataSource
    {
        public string SourceName { get; set; }
        public string Account { get; set; }
        public string Broker { get; set; }
        public bool IsDemo { get; set; }

        public MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        public MarketSeries GetMarketSeries(string key)
        {
            throw new NotImplementedException();
        }

        public MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, DateTime? startDate = default(DateTime?), DateTime? endDate = default(DateTime?))
        {
            throw new NotImplementedException();
        }

        public MarketSeries GetMarketSeries(string key, DateTime? startDate = default(DateTime?), DateTime? endDate = default(DateTime?))
        {
            throw new NotImplementedException();
        }
    }
}

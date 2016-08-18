using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    
    public class LiveMarket : MarketBase, IMarket
    {
        #region IMarket Implementation

        public bool IsBacktesting { get { return false; } }

        public bool IsSimulation {
            get {
                return false;
            }
        }

        #endregion

        #region Parameters

        public TradingAccount Account { get; set; }

        
        public Dictionary<string, TradingAccount> Accounts { get; set; }

        #endregion

        public DateTime SimulationTime {
            get {
                return DateTime.UtcNow; // FUTURE: use server time
            }
        }

        public TimeZoneInfo TimeZone {
            get {
                return TimeZoneInfo.Utc;
            }
        }

        #region Derived

        public bool IsRealMoney {
            get {
                return Account != null && !Account.IsDemo;
            }
        }

        #endregion


        #region Uplink

        public IEnumerable<string> SymbolsAvailable {
            get {
                yield break;
            }
        }

        public IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol)
        {
            return Enumerable.Empty<string>();
        }

        public MarketSeries GetMarketSeries(string symbol, TimeFrame tf)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}

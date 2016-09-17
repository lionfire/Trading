using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
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

        IAccount IMarket.Account { get { return Account; } }
        public LiveAccount Account { get; set; }

        
        public List<IAccount> Accounts { get; set; }
        //public Dictionary<string, Accounts> LiveAccounts { get; set; }

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

        public IObservable<bool> Started { get { return started; } }
        BehaviorSubject<bool> started = new BehaviorSubject<bool>(false);

        #region Derived

        public bool IsRealMoney {
            get {
                return Account != null && !Account.IsDemo;
            }
        }

        #endregion

        public void Add(IMarketParticipant p)
        {
        }

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

        public Symbol GetSymbol(string symbolCode)
        {
            throw new NotImplementedException();
        }
        public override MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        

    }
}

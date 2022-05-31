// OLD FILE
//using LionFire.Reactive;
//using LionFire.Reactive.Subjects;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reactive.Subjects;
//using System.Threading.Tasks;

//namespace LionFire.Trading
//{
    
    
//    public abstract class LiveMarket : MarketBase<TMarket>, IAccount
//    {
//        #region IAccount Implementation

//        public bool IsBacktesting { get { return false; } }

//        public bool IsSimulation {
//            get {
//                return false;
//            }
//        }

//        #endregion

//        #region Parameters

//        IAccount IAccount.Account { get { return Account; } }
//        public abstract IAccount Account { get; }

        
//        public List<IAccount> Accounts { get; set; }
//        //public Dictionary<string, Accounts> LiveAccounts { get; set; }

//        #endregion

//        public DateTime SimulationTime {
//            get {
//                return DateTime.UtcNow; // FUTURE: use server time
//            }
//        }

//        public TimeZoneInfo TimeZone {
//            get {
//                return TimeZoneInfo.Utc;
//            }
//        }

//        public IBehaviorObservable<bool> Started { get { return started; } }
//        protected BehaviorObservable<bool> started = new BehaviorObservable<bool>(false);

//        #region Derived

//        public bool IsRealMoney {
//            get {
//                return Account != null && !Account.IsDemo;
//            }
//        }

//        #endregion

        

//        internal void Start()
//        {
//            throw new NotImplementedException();
//        }

//        #region Uplink

//        public IEnumerable<string> SymbolsAvailable {
//            get {
//                return BrokerInfoUtils.GetSymbolsAvailable(Account?.Template?.Exchange);
//            }
//        }

//        public IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol)
//        {
//            return Enumerable.Empty<string>();
//        }

//        #endregion

//    }
//}

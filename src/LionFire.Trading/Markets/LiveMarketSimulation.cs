using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class LiveMarketSimulation : SimulatedMarketBase, IMarket
    {
        #region IMarket Implementation

        public override bool IsBacktesting { get { return false; } }

        

        public bool IsRealMoney {
            get {
                return false;
            }
        }

        #endregion

        public DateTime SimulationTime {
            get; set;
        }

        public TimeZoneInfo TimeZone {
            get {
                return TimeZoneInfo.Utc;
            }
        }

        public void SimulateBar(SymbolBar bar)
        {
            Console.WriteLine(bar);
        }
        public void OnBars(IEnumerable<SymbolBar> bars)
        {
            foreach (var bar in bars)
            {
                SimulateBar(bar);

            }
        }

        #region Methods

        public void Run()
        {

        }

        public void RunTo(DateTime dateTime)
        {

        }

        #endregion

        #region Subscriptions

        private Dictionary<string, List<WeakReference>> subscriptions = new Dictionary<string, List<WeakReference>>();

        //ConditionalWeakTable<object, List<string>> subscriptions = new ConditionalWeakTable<object, List<string>>();

        public void Subscribe(string symbol, object obj, TimeFrame tf)
        {
            var key = symbol + ";" + tf;

            List<WeakReference> list;
            if (!subscriptions.TryGetValue(key, out list))
            {
                list = new List<WeakReference>();
                subscriptions.Add(key, list);
            }

            list.Add(new WeakReference(obj));
        }

        public void PurgeSubscriptions()
        {
            foreach (var kvp in subscriptions.ToArray())
            {
                bool isAlive = false;
                foreach (var wr in kvp.Value)
                {
                    if (wr.IsAlive)
                    {
                        isAlive = true;
                        break;
                    }
                }
                if (!isAlive)
                {
                    subscriptions.Remove(kvp.Key);
                    var chunks = kvp.Key.Split(';');
                    var symbol = chunks[0];
                    var tf = TimeFrame.TryParse(chunks[1]);
                    OnUnsubscribed(symbol, tf);
                }
            }
        }

        protected virtual void OnSubscribed(string symbol, TimeFrame tf)
        {
        }

        protected virtual void OnUnsubscribed(string symbol, TimeFrame tf)
        {
        }

        #endregion


        //public ISet<string> ActiveSubscriptions {
        //    get {
        //        var set = new HashSet<string>();
        //        foreach (var sub in subscriptions.)
        //        {

        //            if(!set.Contains(sub
        //            set.Add
        //        }
        //    }
        //}

    }
}

#if NET462
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Spotware.Connect;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Spotware.Connect
{
    public class CTraderSymbol : SymbolImplBase
    {
        CTraderAccount account;

        public CTraderSymbol(string symbolCode, CTraderAccount account) : base(symbolCode, account)
        {
            this.account = account;
        }

        Subject<TimedTick> subject;

#region Handle Data from Server

        internal void Handle(TimedBar bar)
        {
        }


        internal void Handle(TimedTick tick)
        {
            var msg = this.Code;
            if (tick.HasAsk)
            {
                msg += " Ask: " + tick.Ask;
                this.Ask = tick.Ask;
            }
            if (tick.HasBid) {
                msg += " Bid: " + tick.Bid;
                this.Bid = tick.Bid;
            }
            
            account.Logger.LogTrace(msg);

            if (subject != null && subject.HasObservers)
            {
                subject.OnNext(tick);
            }
            else
            {
                // Unsubscribe to reduce network traffic
            }
        }

#endregion



#region Account Current Positions

        public override double UnrealizedGrossProfit {
            get {
                double sum = 0.0;
                foreach (var position in account.Positions.Where(p => p.Symbol.Code == this.Code))
                {
                    sum += position.GrossProfit;
                }
                return sum;
            }
        }

        public override double UnrealizedNetProfit {
            get {
                double sum = 0.0;
                foreach (var position in account.Positions.Where(p => p.Symbol.Code == this.Code))
                {
                    sum += position.NetProfit;
                }
                return sum;
            }
        }

#endregion

    }
}

#endif
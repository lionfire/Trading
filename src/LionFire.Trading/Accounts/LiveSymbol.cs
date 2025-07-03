//#if NET462
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading.Tasks;
using LionFire.ExtensionMethods;

namespace LionFire.Trading.Spotware.Connect
{
    public class SimulatedSymbol<AccountType> : SymbolImplBase<AccountType>, INotifyPropertyChanged
        where AccountType : IFeed_Old
    {
        #region Construction

        public SimulatedSymbol(string symbolCode, AccountType account) : base(symbolCode, account)
        {
        }

        public override Task<TimedBar> GetLastBar(TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        public override Task<Tick> GetLastTick()
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    public abstract class LiveSymbol<AccountType> : SymbolImplBase<AccountType>, INotifyPropertyChanged
        where AccountType : IFeed_Old
    {

        #region Construction

        public LiveSymbol(string symbolCode, AccountType account) : base(symbolCode, account)
        {
        }

        #endregion

        #region Observables

        public IObservable<Tick> Ticks { get { return tickSubject; } }
        Subject<Tick> tickSubject = new Subject<Tick>();

        public IObservable<TimedBar> GetBars(TimeFrame timeFrame)
        {
            return barSubjects.GetOrAdd(timeFrame.Name, _ => new Subject<TimedBar>());
        }
        private Dictionary<string, Subject<TimedBar>> barSubjects = new Dictionary<string, Subject<TimedBar>>();

        #region Convenience

        public IObservable<TimedBar> M1Bars { get { return GetBars(TimeFrame.m1); } }
        public IObservable<TimedBar> H1Bars { get { return GetBars(TimeFrame.h1); } }

        #endregion

        #endregion

        #region Handle Data from Server

        //internal void Handle(TimeFrameBar bar)
        //{
        //    Console.WriteLine("ToDo: Handle TimeFrameBar: " + bar);

        //}

        internal void Handle(Tick tick)
        {
            //var msg = this.Code;
            if (tick.HasAsk)
            {
                //msg += " Ask: " + tick.Ask;
                this.Ask = tick.Ask;
            }
            if (tick.HasBid)
            {
                //msg += " Bid: " + tick.Bid;
                this.Bid = tick.Bid;
            }

            //account.Logger.LogTrace(msg);

            if (tickSubject != null && tickSubject.HasObservers)
            {
                tickSubject.OnNext(tick);
            }
            else
            {
                // Unsubscribe to reduce network traffic
            }
        }

        #endregion

        #region Account Current Positions

        public override double UnrealizedGrossProfit
        {
            get
            {
                if (Account == null) return double.NaN;
                return Account.UnrealizedGrossProfit(Code);
            }
        }

        public override double UnrealizedNetProfit
        {
            get
            {
                if (Account == null) return double.NaN;
                return Account.UnrealizedNetProfit(Code);
            }
        }

        #endregion

    }
    
}

//#endif
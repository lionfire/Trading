#if NET462
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Spotware.Connect;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using LionFire.ExtensionMethods;
using System.Reactive;

namespace LionFire.Trading.Spotware.Connect
{
    public class SeriesObservable<T> : SubjectBase<T>
    {
        public override bool HasObservers {
            get {
                throw new NotImplementedException();
            }
        }

        public override bool IsDisposed {
            get {
                throw new NotImplementedException();
            }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public override void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public override void OnNext(T value)
        {
            throw new NotImplementedException();
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }
    public class X<T> : ObservableBase<T>
    {
        protected override IDisposable SubscribeCore(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }

    public class LiveSymbol<TAccount> : SymbolImplBase
        where TAccount : IAccount, IMarket
    {
        protected TAccount account;

        #region Construction

        public LiveSymbol(string symbolCode, TAccount account) : base(symbolCode, account)
        {
            System.Reactive.AnonymousObservable<TimedBar> a;

            this.account = account;
        }

        #endregion

        #region Observables

        public IObservable<TimedTick> Ticks { get { return tickSubject; } }
        Subject<TimedTick> tickSubject;

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
            if (tick.HasBid)
            {
                msg += " Bid: " + tick.Bid;
                this.Bid = tick.Bid;
            }

            account.Logger.LogTrace(msg);

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

    public class CTraderSymbol : LiveSymbol<CTraderAccount>
    {
        #region Construction

        public CTraderSymbol(string symbolCode, CTraderAccount account) : base(symbolCode, account)
        {
        }
        
        #endregion
    }
}

#endif
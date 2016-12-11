//#if NET462
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
using System.Collections.Concurrent;

namespace LionFire.Trading.Spotware.Connect
{
    public class SeriesObservable<T> : SubjectBase<T>
    {
        public override bool HasObservers
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsDisposed
        {
            get
            {
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

    public abstract class LiveSymbol<AccountType> : SymbolImplBase
        where AccountType : IAccount
    {
        public new AccountType Account { get { return account; } }
        protected AccountType account;

        #region Construction

        public LiveSymbol(string symbolCode, AccountType account) : base(symbolCode, account)
        {
            System.Reactive.AnonymousObservable<TimedBar> a;

            this.account = account;
        }

        #endregion

        #region Observables

        public IObservable<Tick> Ticks { get { return tickSubject; } }
        Subject<Tick> tickSubject;

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

        internal void Handle(TimeFrameBar bar)
        {
            Console.WriteLine("ToDo: Handle TimeFrameBar: " + bar);

        }

        internal void Handle(Tick tick)
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

        public override double UnrealizedGrossProfit
        {
            get
            {
                double sum = 0.0;
                foreach (var position in account.Positions.Where(p => p.Symbol.Code == this.Code))
                {
                    sum += position.GrossProfit;
                }
                return sum;
            }
        }

        public override double UnrealizedNetProfit
        {
            get
            {
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
            var symbolInfo = BrokerInfoUtils.GetSymbolInfo(account.Template.BrokerName, symbolCode);
            this.LoadSymbolInfo(symbolInfo);
        }

        #endregion

        public TimeSpan MaxTimeDifferential = TimeSpan.FromMinutes(5); // TODO Make this smaller, fix my clock
        public override async Task<TimedBar> GetLastBar(TimeFrame timeFrame)
        {
            if (Account.ExtrapolatedServerTime != default(DateTime) && (DateTime.UtcNow - Account.ExtrapolatedServerTime) < MaxTimeDifferential)
            {
                var series = this.GetMarketSeries(timeFrame);
                if ((Account.ExtrapolatedServerTime - series.OpenTime.LastValue) < TimeSpan.FromSeconds(65))
                {
                    return series.LastBar; // Assume m1 subscription is in effect
                }
            }

            var task = new SpotwareLoadHistoricalDataJob(this.Code, timeFrame)
            {
                Account = Account,
                //AccountId = Account.Template.AccountId,
                //AccessToken = Account.Template.AccessToken,
                EndTime = DateTime.UtcNow,
                MinBars = 1,
            };
            await task.Run();
            if (task.Result.Count == 0) return null;
            Account.AdvanceServerTime(task.Result.Last().OpenTime + timeFrame.TimeSpan);
            return task.Result[task.Result.Count - 1];
        }


    }
}

//#endif
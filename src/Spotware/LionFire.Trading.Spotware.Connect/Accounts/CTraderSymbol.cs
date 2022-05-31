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
using System.Diagnostics;
using System.ComponentModel;

namespace LionFire.Trading.Spotware.Connect
{
    //public class SeriesObservable<T> : SubjectBase<T>
    //{
    //    public override bool HasObservers
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public override bool IsDisposed
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public override void Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void OnCompleted()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void OnError(Exception error)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void OnNext(T value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override IDisposable Subscribe(IObserver<T> observer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    //public class X<T> : ObservableBase<T>
    //{
    //    protected override IDisposable SubscribeCore(IObserver<T> observer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CTraderSymbol : LiveSymbol<CTraderAccount>
    {
        #region Construction

        public CTraderSymbol(string symbolCode, CTraderAccount account) : base(symbolCode, account)
        {
            var symbolInfo = BrokerInfoUtils.GetSymbolInfo(account.Template.Exchange, symbolCode);
            this.LoadSymbolInfo(symbolInfo);
        }

        #endregion

        public static readonly TimeSpan DefaultLagDelay = TimeSpan.FromMilliseconds(2000);  // HARDCONST
        public static readonly TimeSpan MaxTimeDifferential = TimeSpan.FromMinutes(5); // HARDCONST - TODO Make this smaller, fix my clock

        public override async Task<TimedBar> GetLastBar(TimeFrame timeFrame)
        {
            if (timeFrame == TimeFrame.t1) { throw new ArgumentException("Can't get last bar for t1"); }

            var series = this.GetMarketSeries(timeFrame);
            if (Account.ExtrapolatedServerTime != default(DateTime) && (DateTime.UtcNow - Account.ExtrapolatedServerTime) < MaxTimeDifferential)
            {
                if ((Account.ExtrapolatedServerTime - series.OpenTime.LastValue) < TimeSpan.FromSeconds(65))
                {
                    return series.Last; // Assume m1 subscription is in effect
                }
            }

            await series.EnsureDataAvailable(null, DateTime.UtcNow, 1).ConfigureAwait(false);

            if (series.Count == 0) return TimedBar.Invalid;
            return series.Last;
        }

        public override async Task<Tick> GetLastTick()
        {
            var series = this.MarketTickSeries;

            DateTime time = default(DateTime);
            double lastBid = double.NaN;
            double lastAsk = double.NaN;

            var minIndex = series.FirstIndex;
            for (int index = series.LastIndex; double.IsNaN(lastBid) || double.IsNaN(lastAsk); index--)
            {
                if (index < series.FirstIndex)
                {
                    await series.LoadMoreData().ConfigureAwait(false);
                    if (index < series.FirstIndex)
                    {
                        System.Diagnostics.Debug.WriteLine("LoadMoreData didn't get any more data.  That must be all that's available");
                        break;
                    }
                }

                var tick = series[index];
                if (time == default(DateTime))
                {
                    time = tick.Time;
                }
                if (double.IsNaN(lastBid))
                {
                    if (!double.IsNaN(Bid))
                    {
                        lastBid = tick.Bid;
                    }
                }
                if (double.IsNaN(lastAsk))
                {
                    if (!double.IsNaN(Ask))
                    {
                        lastAsk = tick.Ask;
                    }
                }
            }

            if (Account.ExtrapolatedServerTime != default(DateTime) && (DateTime.UtcNow - Account.ExtrapolatedServerTime) < MaxTimeDifferential)
            {
                if ((Account.ExtrapolatedServerTime - series.OpenTime.LastValue) < TimeSpan.FromSeconds(65))
                {
                    return series.Last;
                }
            }

            // DefaultLagDelay is an extra buffer on top of LocalDelta. TODO: Account for StdDev, also see how far in the future Spotware will let me query
            await series.EnsureDataAvailable(series.Last.Time + series.TimeFrame.TimeSpan, DateTime.UtcNow - Feed.LocalDelta + DefaultLagDelay, forceRetrieve:true).ConfigureAwait(false);

            if (series.Count == 0) return Tick.Invalid;
            return new Tick(time, lastBid, lastAsk);
        }
        
    }
    
}

//#endif
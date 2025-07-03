using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LionFire.Extensions.Logging;
using System.Diagnostics;

namespace LionFire.Trading
{
    /// <summary>
    /// Responsibilities
    ///  - Ensure MarketSeries.Last is up to date
    /// </summary>
    public class BarToOtherBarHandler
    {
        ILogger logger;

        public string SymbolCode => MarketSeries.SymbolCode;
        public MarketSeries MarketSeries { get; set; }
        public IAccount_Old Account { get; set; }
        private MarketSeries m1 { get; set; }
        public BarToOtherBarHandler(IAccount_Old account, MarketSeries series)
        {
            this.MarketSeries = series;
            this.Account = account;
            m1 = series.Symbol.GetMarketSeries(TimeFrame.m1);
            logger = this.GetLogger();
        }

        #region IsEnabled

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                if (isEnabled)
                {
                    m1.Bar += M1_Bar;
                    if (useTicks)
                    {
                        MarketSeries.Symbol.Ticked += Symbol_Ticked;
                        UpdateLastBarUpToDate();
                    }
                }
                else
                {
                    m1.Bar -= M1_Bar;
                    MarketSeries.Symbol.Ticked -= Symbol_Ticked;
                    bar = TimedBar.Invalid;
                }
            }
        }
        private bool isEnabled;

        #endregion

        private void UpdateLastBarUpToDate()
        {
            Debug.WriteLine($"[{MarketSeries}] TODO: UpdateLastBarUpToDate() " );
            //TimedBar bar = TimedBar.New;
            //MarketSeries.Tim
        }


        #region UseTicks

        public bool UseTicks
        {
            get { return useTicks; }
            set
            {
                if (useTicks == value) return;

                useTicks = value;
                if (useTicks)
                {
                    if (isEnabled)
                    {
                        MarketSeries.Symbol.Ticked += Symbol_Ticked;
                    }
                }
                else
                {
                    MarketSeries.Symbol.Ticked -= Symbol_Ticked;
                }
            }
        }

        private bool useTicks;

        #endregion

        public bool IsSamePeriod(DateTimeOffset time, DateTimeOffset time2)
        {
            var tf = MarketSeries.TimeFrame;
            var periodStart = MarketSeries.TimeFrame.GetPeriodStart(time);
            var periodEndExclusive = periodStart + tf.TimeSpan;
            return time2 < periodEndExclusive && time2 >= periodStart;
        }

        #region ServerTimeFromTick

        public DateTimeOffset ServerTimeFromM1Bar
        {
            get { return serverTimeFromM1Bar; }
            set
            {
                if (serverTimeFromM1Bar == value) return;
                if (serverM1BarToTimeFrameTime == default) { serverM1BarToTimeFrameTime = value; }

                var oldTime = serverTimeFromM1Bar;
                serverTimeFromM1Bar = value;

                if (!IsSamePeriod(oldTime, serverTimeFromM1Bar))
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(WaitForM1BarToMinuteToFinishInMilliseconds).ConfigureAwait(false);
                        OnTimeFrameRollover(oldTime);
                        serverM1BarToTimeFrameTime = serverTimeFromM1Bar;
                    });
                }
            }
        }
        private DateTimeOffset serverTimeFromM1Bar;
        DateTimeOffset serverM1BarToTimeFrameTime;

        #endregion

        object Lock = new object();
        private void OnTimeFrameRollover(DateTimeOffset previousMinute)
        {
            lock (Lock)
            {
                if (!bar.IsValid) return;
                if (!IsSamePeriod(previousMinute, bar.OpenTime)
                    && bar.OpenTime > previousMinute // Shouldn't happen - TOSANITYCHECK
                    ) return;

                RaiseTimeFrameBar(bar);
            }
        }

        public int WaitForM1BarToMinuteToFinishInMilliseconds = 4000; // TOCONFIG

        TimedBar bar = TimedBar.Invalid;

        object M1BarToTimeFrameBarLock = new object();
        private void RaiseTimeFrameBar(TimedBar bar, bool finishedBar = true)
        {
            lock (M1BarToTimeFrameBarLock)
            {
                //var bar = tickToMinuteBars[symbolCode];
                if (!bar.IsValid) return;

                var partial = finishedBar ? "" : " PARTIAL";
                Debug.WriteLine($"[M1 to {MarketSeries.ToString()}] {bar}{partial}");

                ((IMarketSeriesInternal)this.MarketSeries).OnBar(bar, finishedBar);

                bar = TimedBar.New;
            }
        }

        private void Symbol_Ticked(SymbolTick _)
        {
            M1_Bar(m1.Last, true);
            //((IMarketSeriesInternal)this.MarketSeries).OnBar(bar, false);
        }

        private void M1_Bar(TimedBar m1Bar)
        {
            M1_Bar(m1Bar, false);
        }
        private void M1_Bar(TimedBar m1Bar, bool isPartial)
        {
            var serverTime = Account.ServerTime;
            //if (!IsSamePeriod(m1Bar.Time, Account.ServerTime) && m1Bar.Time < Account.ServerTime)
            //{
            //    logger.LogWarning($"[BAR] Got old {SymbolCode} bar for time {m1Bar.Time } when server time is {Account.ServerTime}");
            //}

            if (m1Bar.OpenTime > ServerTimeFromM1Bar)
            {
                ServerTimeFromM1Bar = m1Bar.OpenTime; // May trigger a bar from previous minute, for all symbols, after a delay (to wait for remaining ticks to come in)
            }

            //#if FromTickToMinute
            if (bar.IsValid && !IsSamePeriod(bar.OpenTime, m1Bar.OpenTime))
            {
                // Immediately Trigger a finished bar even after starting the timer above.
                RaiseTimeFrameBar(bar);
                bar = TimedBar.Invalid;
            }

            if (!bar.IsValid)
            {
                bar = new TimedBar()
                {
                    OpenTime = MarketSeries.TimeFrame.GetPeriodStart(m1Bar.OpenTime),
                    Open = m1Bar.Open,
                    High = m1Bar.High,
                    Low = m1Bar.Low,
                    Close = m1Bar.Close,
                    Volume = m1Bar.Volume,
                };
            }
            else
            {
                bar = bar.Merge(m1Bar);
            }

            if (double.IsNaN(bar.Volume)) // REVIEW - is this correct for volume?
            {
                bar.Volume = 1;
            }
            else
            {
                bar.Volume++;
            }
        }

    }
}

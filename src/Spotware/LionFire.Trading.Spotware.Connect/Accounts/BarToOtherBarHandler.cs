using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect
{
    internal class BarToOtherBarHandler
    {
        public MarketSeries MarketSeries { get; set; }
        public IAccount Account { get; set; }
        private MarketSeries m1 { get; set; }
        public BarToOtherBarHandler(IAccount account, MarketSeries series)
        {
            this.MarketSeries = series;
            this.Account = account;
            m1 = series.Symbol.GetMarketSeries(TimeFrame.m1);
            
        }

        #region IsEnabled

        public bool IsEnabled
        {
            get { return isEnabled; }
            set {
                if (isEnabled == value) return;
                isEnabled = value;
                if (isEnabled)
                {
                    m1.Bar += M1_Bar;
                }
                else
                {
                    m1.Bar -= M1_Bar;
                    throw new NotImplementedException("TODO: Deinitialize");
                }
            }
        }
        private bool isEnabled;

        #endregion



        private void M1_Bar(SymbolBar obj)
        {
            throw new NotImplementedException("M1 bar to " + MarketSeries.TimeFrame);

#if FromTickToMinute
            if (!IsSameMinute(obj.Time, ServerTimeFromTick) && obj.Time < serverTickToMinuteTime)
            {
                logger.LogWarning($"[TICK] Got old {obj.Symbol} tick for time {obj.Time} when server tick to minute time is {serverTickToMinuteTime} and server time from tick is {ServerTimeFromTick}");
            }

            if (obj.Time > ServerTimeFromTick)
            {
                ServerTimeFromTick = obj.Time; // May trigger a bar from previous minute, for all symbols, after a delay (to wait for remaining ticks to come in)
            }

            TimedBar bar = tickToMinuteBars.TryGetValue(obj.Symbol, TimedBar.Invalid);
            if (bar.IsValid && !IsSameMinute(bar.OpenTime, obj.Time))
            {
                // Immediately Trigger a finished bar even after starting the timer above.
                TickToMinuteBar(obj.Symbol, bar);
                bar = TimedBar.Invalid;
            }

            if (!bar.IsValid)
            {
                var minuteBarOpen = new DateTime(obj.Time.Year, obj.Time.Month, obj.Time.Day, obj.Time.Hour, obj.Time.Minute, 0);

                bar = new TimedBar()
                {
                    OpenTime = minuteBarOpen,
                };
            }

            if (!double.IsNaN(obj.Bid))
            {
                if (double.IsNaN(bar.Open))
                {
                    bar.Open = obj.Bid;
                }
                bar.Close = obj.Bid;
                if (double.IsNaN(bar.High) || bar.High < obj.Bid)
                {
                    bar.High = obj.Bid;
                }
                if (double.IsNaN(bar.Low) || bar.Low > obj.Bid)
                {
                    bar.Low = obj.Bid;
                }
            }

            if (double.IsNaN(bar.Volume)) // REVIEW - is this correct for volume?
            {
                bar.Volume = 1;
            }
            else
            {
                bar.Volume++;
            }

            if (tickToMinuteBars.ContainsKey(obj.Symbol))
            {
                tickToMinuteBars[obj.Symbol] = bar;
            }
            else
            {
                tickToMinuteBars.Add(obj.Symbol, bar);
            }
#endif
        }

    }
}

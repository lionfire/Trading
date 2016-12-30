//#define TRACE_INDICATOR_BARS
#if DEBUG
#define NULLCHECKS
#endif
#if cAlgo
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{

    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, ISingleSeriesIndicator
            where TConfig : ITSingleSeriesIndicator, new()
    {

#if !cAlgo
        public override IEnumerable<MarketSeries> MarketSeriesOfInterest
        {
            get
            {
                yield return MarketSeries;
            }
        }
#endif

        #region Construction

        public SingleSeriesIndicatorBase() { }

        public SingleSeriesIndicatorBase(TConfig config) : base(config)
        {
        }

        #endregion

        protected IDataSeries DataSeries;

        protected override void OnInitializing()
        {
            base.OnInitializing();
            OnInitializing_();

            DataSeries = MarketSeries.GetDataSeries(Template.IndicatorBarSource);
        }
        partial void OnInitializing_();


        public override void CalculateToTime(DateTime date)
        {
#if cAlgo
            var series = Bot == null ? MarketSeries : Bot.MarketSeries;
            if (MarketSeries == null && Bot == null)
            {
                throw new ArgumentNullException("MarketSeries == null && Bot == null");
            }
#else
            var series = MarketSeries;
#endif
#if NULLCHECKS
            if (series == null)
            {
                throw new ArgumentNullException("MarketSeries");
            }
#endif

            //l.Debug("Calculating until " + date);

            for (int index = CalculatedCount; series.OpenTime[index] < date; index++)
            {
                if (index >= series.OpenTime.Count) break;
                var openTime = series.OpenTime[index];
                //l.Warn($"series.OpenTime[index] {openTime} open: {series.Open[index]}");
                if (double.IsNaN(series.Open[index])) continue;
                Calculate(index);
            }
            //l.Info("Calculated until " + date + " " + OpenLongPoints.LastValue);
        }


        protected MarketSeries series
        {
            get
            {
#if cAlgo
                return Bot == null ? this.MarketSeries : Bot.MarketSeries;
#else
                return this.MarketSeries;
#endif
            }
            // add set to make it faster?
        }

#if !cAlgo
        public virtual int Periods
        {
            get
            {
                var maxPeriods = 0;
                foreach (var child in Children.OfType<ISingleSeriesIndicator>())
                {
                    maxPeriods = Math.Max(maxPeriods, child.Periods);
                }
                return maxPeriods;
            }
        }
#endif

        public override void Calculate(int upToIndex)
        {
            
#if !cAlgo
            bool failedToGetHistoricalData = false;

            if (CalculatedCount == 0 && !Account.IsBacktesting)
            {

                var minIndex = Math.Max(upToIndex, MarketSeries.LastIndex - Periods);
                //for (int index = MarketSeries.MinIndex; index < minIndex; index++)
                for (int index = MarketSeries.FirstIndex; index < upToIndex - 1; index++)
                {
                    // Skip unneeded sections
                    SetBlank(index); // MEMORYOPTIMIZE use sparse arrays instead of filling with blanks
                }
            }
#endif

            var lastIndex = LastIndex == int.MinValue ? -1 : LastIndex;
            for (int index = lastIndex + 1; index <= upToIndex; index++)
            {
#if !cAlgo
                #region Get Historical Data if needed (REFACTOR to base class)

                var lookbackIndex = index - Periods;

                if (lookbackIndex < MarketSeries.FirstIndex)
                {
                    if (!failedToGetHistoricalData)
                    {
                        Debug.WriteLine($"[indicator for {MarketSeries}] Index needed: {lookbackIndex} + but MarketSeries.MinIndex: {MarketSeries.FirstIndex}.  Requesting {2 + MarketSeries.FirstIndex - lookbackIndex} bars from {MarketSeries.OpenTime.First()}");

                        Task.Run(async () => await MarketSeries.EnsureDataAvailable(null, MarketSeries.OpenTime.First(), 2 + MarketSeries.FirstIndex - lookbackIndex).ConfigureAwait(false)).Wait();

                    }
                    if (lookbackIndex < MarketSeries.FirstIndex)
                    {
                        failedToGetHistoricalData = true; // REVIEW
                                                          // Market data not available
                        SetBlank(index);
                        continue;
                    }
                }
                #endregion
#endif

                CalculateIndex(index);
            }
        }

        protected abstract void CalculateIndex(int index);

#if TRACE_INDICATOR_BARS
        long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (i++ % 111 == 0)
            {
                l.LogInformation($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            }
        }
#endif
    }
}

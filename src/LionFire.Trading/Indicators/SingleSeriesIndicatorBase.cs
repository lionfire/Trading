//#define TRACE_INDICATOR_BARS
#if DEBUG
#define NULLCHECKS
#endif
#if cAlgo
using cAlgo.API.Internals;
using DataSeriesType = cAlgo.API.DataSeries;
#else
using DataSeriesType = LionFire.Trading.DataSeries;
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

//#if !cAlgo
//        public override IEnumerable<MarketSeries> MarketSeriesOfInterest
//        {
//            get
//            {
//                yield return MarketSeries;
//            }
//        }
//#endif

        #region Relationships

        #region Derived

        protected override MarketSeries series
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

        #endregion

        #endregion

        #region Construction

        public SingleSeriesIndicatorBase() { }

        public SingleSeriesIndicatorBase(TConfig config) : base(config)
        {
        }

        #endregion

        protected DataSeriesType DataSeries;

        protected override void OnInitializing()
        {
            base.OnInitializing();
            OnInitializing_();

            DataSeries = Template.IndicatorBarSource ?? MarketSeries.GetDataSeries(Template.IndicatorBarComponent);

        }
        partial void OnInitializing_();
        

        public virtual int Periods
        {
            get
            {
                var t = Template as ITSingleSeriesIndicator;
                if (t != null)
                {
                    return t.Periods;
                }

                var maxPeriods = 0;
                foreach (var child in Children.OfType<IIndicator>().Select(c=>c.Template).OfType<ITSingleSeriesIndicator>())
                {
                    maxPeriods = Math.Max(maxPeriods, child.Periods);
                }
#if DEBUG
                if (maxPeriods == 0)
                {
                    Debug.WriteLine("WARNING: SingleSeriesIndicatorBase.Periods returning 0 for type: " + this.GetType().Name);
                }
#endif
                return maxPeriods;
            }
        }

        public async Task CalculateUpToIndex(int upToIndex) // UNUSED
        {

#if !cAlgo
            bool failedToGetHistoricalData = false;

            //if (CalculatedCount == 0 && !Account.IsBacktesting)
            //{

            //    var minIndex = Math.Max(upToIndex, MarketSeries.LastIndex - Periods);
            //    //for (int index = MarketSeries.MinIndex; index < minIndex; index++)
            //    for (int index = MarketSeries.FirstIndex; index < upToIndex - 1; index++)
            //    {
            //        // Skip unneeded sections
            //        SetBlank(index); // MEMORYOPTIMIZE use sparse arrays instead of filling with blanks
            //    }
            //}
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

                        await MarketSeries.EnsureDataAvailable(null, MarketSeries.OpenTime.First(), 2 + MarketSeries.FirstIndex - lookbackIndex).ConfigureAwait(false);
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

                await CalculateIndex(index).ConfigureAwait(false);
            }
        }


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

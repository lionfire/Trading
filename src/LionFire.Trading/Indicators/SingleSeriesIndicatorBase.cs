#if DEBUG
#define NULLCHECKS
#endif
#if cAlgo
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries
            where TConfig : ITIndicator, new()
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

        protected override void OnInitializing()
        {
            base.OnInitializing();
            OnInitializing_();

        }
        partial void OnInitializing_();

        protected abstract int CalculatedCount { get;
        }

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

        
    }
}

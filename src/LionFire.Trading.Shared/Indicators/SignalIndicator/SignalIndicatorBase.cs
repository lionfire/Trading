#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SignalIndicatorBase<TConfig> : SingleSeriesIndicatorBase<TConfig>, ISignalIndicator
        where TConfig : ITSignalIndicator, ITSingleSeriesIndicator, new()
    {
        #region Construction

        public SignalIndicatorBase() { }
        public SignalIndicatorBase(TConfig config) : base(config) { }

        #endregion

        //protected override int CalculatedCount
        //{
        //    get
        //    {
        //        if (OpenLongPoints != null) return OpenLongPoints.Count;
        //        if (OpenShortPoints != null) return OpenShortPoints.Count;
        //        return 0;
        //    }
        //}

        public override IEnumerable<IndicatorDataSeries> Outputs
        {
            get
            {
                yield return CloseLongPoints;
                yield return CloseShortPoints;
                yield return LongStopLoss;
                yield return OpenLongPoints;
                yield return OpenShortPoints;
                yield return ShortStopLoss;
                
            }
        }

        public IndicatorDataSeries CloseLongPoints
        {
            get; protected set;
        }

        public IndicatorDataSeries CloseShortPoints
        {
            get; protected set;
        }

        public IndicatorDataSeries LongStopLoss
        {
            get; protected set;
        }

        public IndicatorDataSeries OpenLongPoints
        {
            get; protected set;
        }

        public IndicatorDataSeries OpenShortPoints
        {
            get; protected set;
        }

        public IndicatorDataSeries ShortStopLoss
        {
            get; protected set;
        }



    }
}

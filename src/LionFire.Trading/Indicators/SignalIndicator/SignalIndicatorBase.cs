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
        where TConfig : ITSignalIndicator, new()
    {
        #region Construction

        public SignalIndicatorBase() { }
        public SignalIndicatorBase(TConfig config) : base(config) { }

        #endregion


        public IndicatorDataSeries CloseLongPoints {
            get; protected set;
        }

        public IndicatorDataSeries CloseShortPoints {
            get; protected set;
        }

        public IndicatorDataSeries LongStopLoss {
            get; protected set;
        }

        public IndicatorDataSeries OpenLongPoints {
            get; protected set;
        }

        public IndicatorDataSeries OpenShortPoints {
            get; protected set;
        }

        public IndicatorDataSeries ShortStopLoss {
            get; protected set;
        }


    }
}

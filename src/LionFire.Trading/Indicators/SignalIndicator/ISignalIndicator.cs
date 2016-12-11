#if cAlgo
using cAlgo.API;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Indicators;

namespace LionFire.Trading
{
    
    public interface ISignalIndicator : IIndicator, IHasSingleSeries
    {
        

        IndicatorDataSeries OpenShortPoints {
            get;
        }
        IndicatorDataSeries OpenLongPoints {
            get;
        }
        IndicatorDataSeries LongStopLoss {
            get;
        }
        IndicatorDataSeries CloseShortPoints {
            get;
        }
        IndicatorDataSeries CloseLongPoints {
            get;
        }
        IndicatorDataSeries ShortStopLoss {
            get;
        }

        
    }
}

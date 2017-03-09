#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IIndicator : ITemplateInstance
#if !cAlgo
        , IStartable
        , IInitializable
        , IAccountParticipant
#endif
    {
        new ITIndicator Template { get; set; }

        Task CalculateIndex(int index);

        Symbol Symbol { get; }

        Task CalculateToTime(DateTime openTime);

#if !cAlgo
        int GetDesiredBars(string symbolCode, TimeFrame timeFrame);
#endif

    }

}
namespace LionFire.Trading.Indicators
{
    public interface IMovingAverageIndicator : IIndicator
    {
        MovingAverageType Kind { get; }

#if cAlgo
        IndicatorDataSeries Result { get; }
#else
        DataSeries Result { get; }
#endif
    }

}

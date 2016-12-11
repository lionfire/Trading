#if cAlgo
using cAlgo.API.Internals;
#endif
using LionFire.Execution;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public interface IIndicator : ITemplateInstance, IStartable
#if !cAlgo
        , IAccountParticipant
#endif
    {
        new ITIndicator Template { get; set; }

        void Calculate(int index);

        Symbol Symbol { get; }

        void CalculateToTime(DateTime openTime);

#if !cAlgo
        int GetDesiredBars(string symbolCode, TimeFrame timeFrame);
#endif

    }

}

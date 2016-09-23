#if cAlgo
using cAlgo.API.Internals;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public interface IIndicator
#if !cAlgo
        : IMarketParticipant
#endif
    {
        ITIndicator Config { get; set; }

        void Calculate(int index);

        Symbol Symbol { get; }

        void CalculateToTime(DateTime openTime);
    }

}

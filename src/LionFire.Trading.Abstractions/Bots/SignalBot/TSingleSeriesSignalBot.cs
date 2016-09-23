using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class TSingleSeriesSignalBot<TIndicatorConfig> : TSignalBot<TIndicatorConfig>
        //, ISymbolTimeFrameCode
        where TIndicatorConfig : class, ITIndicator
    {

        #region Construction

        public TSingleSeriesSignalBot() { }
        public TSingleSeriesSignalBot(string symbol, string timeFrame)
        {
            this.Symbol = symbol;
            this.TimeFrame = timeFrame;
        }

        #endregion


    }
}

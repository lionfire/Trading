using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    
    public class TSignalBot<TIndicatorType> : TBot
        where TIndicatorType : class, ITIndicator
    {
        
        public double PointsToLong { get; set; } = 1.0;
        public double PointsToShort { get; set; } = 1.0;

        #region Indicator

        public TIndicatorType Indicator { get; set; }

        #endregion

    }
}

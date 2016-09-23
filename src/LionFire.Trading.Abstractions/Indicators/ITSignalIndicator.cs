using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITSignalIndicator : ITIndicator
    {
        double PointsToOpenLong { get; set; }
        double PointsToOpenShort { get; set; }
        double PointsToCloseLong { get; set; }
        double PointsToCloseShort { get; set; }

        [Obsolete]
        double PointsToClose { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum MovingAverageType
    {
        // TOVERIFY
        Simple,
        // TOVERIFY
        Exponential,
        // TODO
        TimeSeries,
        // TOVERIFY
        Triangular,
        /// <summary>
        /// Volitility Index Dynamic Average
        /// TODO
        /// </summary>
        VIDYA,
        // TOVERIFY
        Weighted,
        // TOVERIFY
        WilderSmoothing,
    }
}

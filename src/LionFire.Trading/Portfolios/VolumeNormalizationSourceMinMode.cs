using System;

namespace LionFire.Trading.Portfolios
{
    [Flags]
    public enum VolumeNormalizationReductionMode
    {

        //Unspecified = 0,
        Raw =  0,
        /// <summary>
        /// Example: $4000 USD/CAD becomes 4, assuming a broker minimum trade size of $1000
        /// (Recommended)
        /// </summary>
        DivideByMinimumAllowedTradeVolume = 1 << 0,

        /// <summary>
        /// Example: if a set of trades has smallest trade sizes: $2000, $4000, $5000, divide the smallest by the minimum allowed size, $1000, to get a minimum trade multiple of 2, 
        /// then divide all trades by 2, to get $1000 $2000 and $2500.
        /// Aligns backtesting that was done with an unusually large volume.
        /// (Recommended for doing portfolio analysis.)
        /// </summary>
        DivideByMinimumsMultipleOfMinimumAllowedTradeSize = 1 << 1,
    }
}

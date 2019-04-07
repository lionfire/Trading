//#define ConcurrentInjest
//using BlazorComponents.ChartJS;

namespace LionFire.Trading.Portfolios
{
    public enum PortfolioNormalizationCurveType
    {
        Unspecified = 0,
        /// <summary>
        /// Parameter: if non-zero, multiply values by this
        /// </summary>
        Linear = 1,

        /// <summary>
        /// Paramter: If non-zero, values above the parameter will be max, and values below the parameter will be zero.
        /// </summary>
        Step = 2,

        /// <summary>
        /// Paramter: if non-zero, multiply values by the exponent
        /// </summary>
        EaseIn = 3,
        
        /// <summary>
        /// Paramter: if non-zero, multiply values by the exponent
        /// </summary>
        EaseOut = 4,
    }
}

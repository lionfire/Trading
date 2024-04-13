#if FUTURE
using LionFire.Trading;

namespace LionFire.Trading.Backtesting2;


/// <summary>
/// Listens to a HistoricalTradingContext from a more granular TimeFrame
/// </summary>
public class ChainedHistoricalTradingContext : TradingContext, ITradingContext
{
    #region Configuration

    bool IsLive => false;
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public ChainedHistoricalTradingContext(TimeFrame timeFrame, HistoricalTradingContext parent) : base(parent.ServiceProvider)
    {
        TimeFrame = timeFrame;
    }

    #endregion
}
#endif
using Orleans;
//using static LionFire.Trading.Binance_.UsdFutures24HStatsG;


namespace LionFire.Trading.Binance_;

public interface IUsdFuturesInfoG : IGrainWithStringKey 
{

    ValueTask AutoRefreshInterval(TimeSpan? timeSpan);
    Task<Binance24HStatsState> LastDayStats();

    Task<Binance24HStatsState> RetrieveLastDayStats();
}

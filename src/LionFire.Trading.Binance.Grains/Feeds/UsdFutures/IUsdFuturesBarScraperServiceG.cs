
using Orleans.Concurrency;

namespace LionFire.Trading.Binance_;

//[Alias("BinanceUsdFuturesBarScraperService")]
//[GrainType("BinanceUsdFuturesBarScraperService")]
public interface IUsdFuturesBarScraperServiceG : IGrainWithStringKey
{
    Task Start();

    [ReadOnly]
    Task<int?> MaxSymbols();
    Task MaxSymbols(int? newValue);

    [ReadOnly]
    Task<int> Interval();
    Task Interval(int newValue);
    [ReadOnly]
    Task<int> DisabledInterval();
    Task DisabledInterval(int newValue);

    Task Options(UsdFuturesBarScraperServiceOptions newValue);

    [ReadOnly]
    Task<UsdFuturesBarScraperServiceOptions> Options();
}

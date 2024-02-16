using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LionFire.Trading.Binance_;

namespace LionFire.Trading;

public interface ISymbolStatsCache : IHostedService
{
    public decimal Volume24H(string symbol);
    public int MarketCapRank(string symbol);
}

public class SymbolStatsCache : ISymbolStatsCache
{
    #region Dependencies

    ILogger Logger { get; }
    public IClusterClient ClusterClient { get; }

    #endregion

    #region Lifecycle

    public SymbolStatsCache(ILogger<SymbolStatsCache> logger, IClusterClient clusterClient)
    {
        Logger = logger;
        ClusterClient = clusterClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cts = new();
        timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        Loop().FireAndForget();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts?.Cancel();
        timer?.Dispose();
        timer = null;
        return Task.CompletedTask;
    }

    #endregion

    #region State

    CancellationTokenSource? cts;
    PeriodicTimer? timer;

    public Dictionary<string, Binance24HPriceStats> Binance24HStats => binance24HStats;
    Dictionary<string, Binance24HPriceStats> binance24HStats = new();

    #endregion

    #region Loop

    private async Task Loop()
    {
        while (true && timer != null && cts != null && !cts.Token.IsCancellationRequested)
        {
            try
            {
                await RetrieveAll();
                if (!await timer.WaitForNextTickAsync(cts.Token)) break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception in Loop");
            }
        }
    }

    #endregion

    #region (Public)

    public int MarketCapRank(string symbol)
    {
        return -1;
    }

    public decimal Volume24H(string symbol)
    {
        if (binance24HStats.TryGetValue(symbol, out var stats))
        {
            return stats.QuoteVolume;
        }
        return decimal.MinValue;
    }

    #endregion

    #region (Private) Event Handlers

    private async Task RetrieveAll()
    {
        var g = ClusterClient.GetGrain<IUsdFuturesInfoG>("0");
        binance24HStats = ((await g.LastDayStats()).List ?? []).ToDictionary(i => i.Symbol);

    }

    #endregion

}

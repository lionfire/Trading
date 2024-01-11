using Microsoft.Extensions.Logging;
using Orleans.Runtime;

namespace LionFire.Trading.Binance_;

[Alias("BinanceUsdFuturesBarScraperService")]
public class UsdFuturesBarScraperServiceG([PersistentState("BinanceUsdFuturesBarScraperOptions", "Trading")] IPersistentState<UsdFuturesBarScraperServiceOptions> options, ILogger<UsdFuturesBarScraperServiceG> logger) : Grain, IUsdFuturesBarScraperServiceG
{
    TimeFrame TimeFrame { get; set; } = null!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TimeFrame = TimeFrame.Parse(this.GetPrimaryKeyString());

        return base.OnActivateAsync(cancellationToken);
    }

    public async Task Start()
    {
        if (!options.RecordExists)
        {
            logger.LogInformation("Creating default options");
            options.State = new UsdFuturesBarScraperServiceOptions();
            options.State.MaxSymbols = 2;
            await options.WriteStateAsync();
        }

        var stats = await GrainFactory.GetGrain<IUsdFuturesInfoG>("0").LastDayStats();

        await OnStats(stats.List);

    }

    public const string TFSeparator = "^";
    private async Task OnStats(IEnumerable<Binance24HPriceStats> sortedStats)
    {
        var tfString = TimeFrame.ToShortString();

        int i = 0;

        int enabledStaggerOffset = 0;
        int disabledStaggerOffset = 0;

        foreach (var s in sortedStats)
        {
            i++;
            var g = GrainFactory.GetGrain<IUsdFuturesBarScraperG>(s.Symbol + TFSeparator + tfString);

            var enabled = i <= options.State.MaxSymbols;

            var interval = enabled ? options.State.Interval : options.State.DisabledInterval;
            var offset = enabled ? enabledStaggerOffset : disabledStaggerOffset;
            logger.LogInformation("{symbol}^{tf} interval: {bars} bars (offset: {offset})", s.Symbol, tfString, interval.ToString() ?? "(disabled)", offset);

            await g.Interval(interval);

            if (interval > 0)
            {
                await g.Offset(offset);
                if (enabled) { enabledStaggerOffset++; enabledStaggerOffset %= interval; }
                else { disabledStaggerOffset++; disabledStaggerOffset %= interval; }
            }

            await g.Init();
        }
    }

    public Task<int?> MaxSymbols() => Task.FromResult(options.State.MaxSymbols);
    public async Task MaxSymbols(int? newValue)
    {
        options.State.MaxSymbols = newValue;
        await options.WriteStateAsync();
    }

    public Task<int> Interval() => Task.FromResult(options.State.Interval);
    public async Task Interval(int newValue)
    {
        options.State.Interval = newValue;
        await options.WriteStateAsync();
    }
    public Task<int> DisabledInterval() => Task.FromResult(options.State.DisabledInterval);
    public async Task DisabledInterval(int newValue)
    {
        options.State.DisabledInterval = newValue;
        await options.WriteStateAsync();
    }
}

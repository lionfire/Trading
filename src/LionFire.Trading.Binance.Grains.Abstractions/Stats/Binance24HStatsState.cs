using Orleans;


namespace LionFire.Trading.Binance_;

[Alias("binance-futures-usd-stats-24h")]
[GenerateSerializer]
public class Binance24HStatsState
{
    [Id(0)]
    public DateTimeOffset RetrievedOn { get; set; }
    [Id(1)]
    public List<Binance24HPriceStats>? List { get; set; }
}

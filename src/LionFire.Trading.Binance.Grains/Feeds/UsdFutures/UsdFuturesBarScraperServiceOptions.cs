namespace LionFire.Trading.Binance_;

[GenerateSerializer]
[Alias("UsdFuturesBarScraperServiceOptions")]
public class UsdFuturesBarScraperServiceOptions
{
    /// <summary>
    /// Null: no max
    /// </summary>
    [Id(0)]
    public int? MaxSymbols { get; set; }

    [Id(1)]
    public int Interval { get; set; } = 1;
    [Id(2)]
    public int DisabledInterval { get; set; } = 0;

  
}

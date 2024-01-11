namespace LionFire.Trading.Binance_;

[Alias("BinanceUsdFuturesBarScraperOptions")]
[GenerateSerializer]
public class UsdFuturesBarScraperOptions
{
    /// <summary>
    /// Scrape every {Interval} bars.  0 = disabled
    /// </summary>
    [Id(0)]
    public int Interval { get; set; }

    /// <summary>
    /// Scrape every {Interval} bars
    /// </summary>
    [Id(1)]
    public int Offset { get; set; }

    /// <summary>
    /// For TimeFrames longer than m1, stagger the retrieve by up to {DisabledStaggerMinutes} minutes
    /// </summary>
    /// <example>
    /// 
    /// E.g. 
    ///  - TF: h1
    ///  - # of disabled symbols: 20
    ///  - DisabledStaggerMinutes: 10 (recommended: 60 for h1)
    ///  - DisabledSkipMinutes: [0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55] 
    ///  
    /// Time progression:
    /// 
    /// :00 none
    /// :01 S1 S10 S18
    /// :02 S2 S11 S19
    /// :03 S3 S12
    /// :04 S4 S13
    /// :05 none
    /// :06 S5 S14
    /// :07 S6 S15
    /// :08 S7 S16
    /// :09 S9 S17
    /// 
    /// </example>
    [Id(2)]
    public int DisabledStaggerMinutes { get; set; }
    [Id(3)]
    public int[]? DisabledSkipMinutes { get; set; }
}

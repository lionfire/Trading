namespace LionFire.Trading.Binance_;

[Immutable]
[GenerateSerializer]
public class Binance24HPriceStats
{
    [Id(0)]
    public string Symbol { get; set; }
    [Id(1)]
    public decimal QuoteVolume { get; set; }


    /// <summary>
    /// The actual price change in the last 24 hours
    /// </summary>
    [Id(2)]
    public decimal PriceChange { get; set; }

    /// <summary>
    /// The price change in percentage in the last 24 hours
    /// </summary>
    [Id(3)]
    public decimal PriceChangePercent { get; set; }

    /// <summary>
    /// The weighted average price in the last 24 hours
    /// </summary>
    [Id(4)]
    public decimal WeightedAveragePrice { get; set; }

    /// <summary>
    /// The most recent trade quantity
    /// </summary>
    [Id(5)]
    public decimal LastQuantity { get; set; }

    /// <summary>
    /// Time at which this 24 hours opened
    /// </summary>
    [Id(6)]
    public DateTime OpenTime { get; set; }

    /// <summary>
    /// Time at which this 24 hours closed
    /// </summary>
    [Id(7)]
    public DateTime CloseTime { get; set; }

    /// <summary>
    /// The first trade ID in the last 24 hours
    /// </summary>
    [Id(8)]
    public long FirstTradeId { get; set; }

    /// <summary>
    /// The last trade ID in the last 24 hours
    /// </summary>
    [Id(9)]
    public long LastTradeId { get; set; }

    /// <summary>
    /// The amount of trades made in the last 24 hours
    /// </summary>
    [Id(10)]
    public long TotalTrades { get; set; }

}

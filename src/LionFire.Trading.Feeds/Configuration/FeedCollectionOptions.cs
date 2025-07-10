using System.Collections.Generic;

namespace LionFire.Trading.Feeds.Configuration;

public class FeedCollectionOptions
{
    public List<string> Symbols { get; set; } = new();
    public bool CollectTrades { get; set; } = true;
    public bool CollectOrderBook { get; set; } = true;
    public bool CollectOnTradeOnly { get; set; } = true;
    public OrderBookDepthOptions OrderBookDepth { get; set; } = new();
}

public class OrderBookDepthOptions
{
    public bool Collect01Percent { get; set; } = true;
    public bool Collect025Percent { get; set; } = true;
    public bool Collect05Percent { get; set; } = true;
    public bool Collect075Percent { get; set; } = true;
    public bool Collect1Percent { get; set; } = true;
    public bool Collect2Percent { get; set; } = true;
}
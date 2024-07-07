namespace LionFire.Trading;

public enum MarketFeatures
{
    Unspecified = 0,
    Exists = 1 << 0,
    Long = 1 << 1,
    Short = 1 << 2,
    Ticks = 1 << 3,
    Bars = 1 << 4,
    OrderBook = 1 << 5,
    //OrderBookDepth = 1 << 6,
    //OrderBookL2 = 1 << 8,

}


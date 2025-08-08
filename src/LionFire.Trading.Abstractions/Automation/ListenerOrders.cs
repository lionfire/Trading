namespace LionFire.Trading.Automation;


public static class ListenerOrders
{
    public const float Default = 0;

    public const float MarketData = -1_000_000f;
    public const float AccountMarket = -100_000f; // Markets for accounts process before accounts
    public const float Account = -10_000f;
    public const float Indicator = -100f;
    public const float Bot = 0f;

    public const float Watcher = 1_000_000f;
}

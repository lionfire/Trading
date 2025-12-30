namespace LionFire.Trading.Alerts;

[Flags]
public enum TradingAlertDomains
{
    Unspecified = 0,

    Price = 1 << 0,
    Volume = 1 << 1,
    OrderBook = 1 << 2,
    Pattern = 1 << 3,
    Indicator = 1 << 4,

    #region User-specific

    Position = 1 << 16,
    Order = 1 << 17,

    #endregion
}

namespace LionFire.Trading.Binance_;

public enum BarStatus
{
    Unspecified = 0,
    Confirmed = 1 << 0,
    Tentative = 1 << 1,
    Revision = 1 << 2,
    InProgress = 1 << 3,
}

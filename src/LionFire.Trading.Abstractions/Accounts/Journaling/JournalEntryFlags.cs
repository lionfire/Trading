namespace LionFire.Trading;

[Flags]
public enum JournalEntryFlags
{
    Unspecified = 0,
    StopLoss = 1 << 0,
    TakeProfit = 1 << 1,
    Reverse = 1 << 10,
}

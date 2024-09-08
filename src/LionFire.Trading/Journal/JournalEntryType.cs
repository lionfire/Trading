namespace LionFire.Trading;

public enum JournalEntryType
{
    Unspecified = 0,
    JournalOpen,
    OpenPosition,
    ClosePosition,
    ModifyPosition,
    CreateOrder,
    ModifyOrder,
    CancelOrder,
    SwapFee,
    InterestFee,
}

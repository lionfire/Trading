namespace LionFire.Trading;

public enum JournalEntryType
{
    Unspecified = 0,
    Open,
    Close,
    Modify,
    CreateOrder,
    ModifyOrder,
    CancelOrder,
    SwapFee,
    InterestFee,
    Abort,
    Start,
    End,
}



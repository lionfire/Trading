namespace LionFire.Trading.Automation;

public abstract record AccountJournalEntry(DateTimeOffset Time);
public record AccountBalanceEntry(DateTimeOffset Time, double Balance) : AccountJournalEntry(Time);
public record AccountEquityEntry(DateTimeOffset Time, HLC<double> Equity) : AccountJournalEntry(Time);

public enum PositionReason
{
    Unspecified,
    Manual,
    StopLoss,
    TakeProfit,
}
public record AccountPositionEntry(DateTimeOffset Time, string Symbol, LongAndShort LongAndShort, double PositionSize, bool Open, PositionReason Reason) : AccountJournalEntry(Time);
public record AccountStopLossEntry(DateTimeOffset Time, string Symbol, LongAndShort LongAndShort, double PositionSize, bool Open) : AccountJournalEntry(Time);


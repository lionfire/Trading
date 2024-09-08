using MemoryPack;
using System.CommandLine;
using System.Numerics;

namespace LionFire.Trading;

[MemoryPackable]
public partial class JournalEntry<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    public JournalEntryType EntryType { get; set; }
    public DateTimeOffset Time { get; set; }

    public string? Context { get; set; }
    public string? Symbol { get; set; }
    public long? PositionId { get; set; }
    public long? OrderId { get; set; }
    public long? TransactionId { get; set; }
    public TPrecision? NetProfit { get; set; }
    public TPrecision? GrossProfit { get; set; }
    public TPrecision? Commission { get; set; }
    public TPrecision? Balance { get; set; }
    public TPrecision? Swap { get; set; }
    public TPrecision? Price { get; set; }
    public TPrecision? EntryAverage { get; set; }
    public TPrecision? Quantity { get; set; }
    public TPrecision? QuantityChange { get; set; }
    public TPrecision RealizedGrossProfitDelta { get; set; }

    [MemoryPackConstructor]
    public JournalEntry() { }

    public JournalEntry(IAccount2<TPrecision> account)
    {
        Balance = account.Balance;
    }

    public JournalEntry(IPosition<TPrecision> position) : this(position.Account)
    {
        PositionId = position.Id;
        Quantity = position.Quantity;
        EntryAverage = position.EntryAverage;
        Symbol = position.Symbol;
    }
    public override string ToString() => this.ToXamlAttribute();
}

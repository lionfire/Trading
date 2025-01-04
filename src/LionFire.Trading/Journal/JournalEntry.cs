using MemoryPack;
using System.CommandLine;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading;

namespace LionFire.Trading;

public abstract class JournalEntry
{
    #region Properties

    public JournalEntryType EntryType { get; set; }
    public JournalEntryFlags Flags { get; set; }
    public DateTimeOffset Time { get; set; }

    public string? Context { get; set; }
    public string? Symbol { get; set; }
    public long? PositionId { get; set; }
    public long? OrderId { get; set; }
    public long? TransactionId { get; set; }

    #endregion
    public abstract float? NetProfitFloat { get; }
    public abstract float? RealizedGrossProfitDeltaFloat { get; }
  
}

    [MemoryPackable]
public partial class JournalEntry<TPrecision> : JournalEntry
    where TPrecision : struct, INumber<TPrecision>
{

    #region Properties

    public TPrecision? NetProfit { get; set; }
    public override float? NetProfitFloat { get => Convert.ToSingle(NetProfit); }

    public TPrecision? GrossProfit { get; set; }
    public TPrecision? Commission { get; set; }
    public TPrecision? Balance { get; set; }
    public TPrecision? Swap { get; set; }
    public TPrecision? Price { get; set; }
    public TPrecision EntryAverage { get; set; }
    public TPrecision? Quantity { get; set; }
    public TPrecision? QuantityChange { get; set; }
    public TPrecision RealizedGrossProfitDelta { get; set; }
    public override float? RealizedGrossProfitDeltaFloat => Convert.ToSingle(RealizedGrossProfitDelta);

    #endregion

    #region Non-serialized

    [MemoryPackIgnore]
    public IPosition<TPrecision>? Position { get; set; }

    #endregion 

    #region Lifecycle

    [MemoryPackConstructor]
    public JournalEntry() { }

    public JournalEntry(IAccount2<TPrecision> account)
    {
        Balance = account.Balance;
    }

    public JournalEntry(IPosition<TPrecision> position) : this(position.Account)
    {
        Position = position;
        PositionId = position.Id;
        Quantity = position.Quantity;
        EntryAverage = position.EntryAverage;
        Symbol = position.Symbol;
    }

    #endregion

    #region Misc

    public override string ToString() => this.ToXamlAttribute();

    #endregion

}

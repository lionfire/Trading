using CsvHelper.Configuration.Attributes;

namespace LionFire.Trading.Automation;

public class BacktestBatchJournalEntry
{
    public static implicit operator BacktestReference(BacktestBatchJournalEntry entry) => new(entry.BatchId, entry.Id);

    public int BatchId { get; set; }

    /// <summary>
    /// Id within a batch
    /// </summary>
    public long Id { get; set; }

    [Ignore]
    public string StringId => $"{BatchId}-{Id}";

    public double Fitness { get; set; }
    public double AD { get; set; }

    /// <summary>
    /// Average minutes per winning trade
    /// </summary>
    public double AMWT { get; set; }
    public int Wins { get; set; }
    public double WinRate => Wins / (double)ClosedTrades;
    public double CloseRate => ClosedTrades / (double)TotalTrades;
    public int Losses { get; set; }
    public int Breakevens { get; set; }
    public int UnknownTrades { get; set; }
    public int TotalTrades => Wins + Losses + Breakevens + UnknownTrades;
    public int ClosedTrades => Wins + Losses + Breakevens;

    /// <summary>
    /// Injected as needed
    /// </summary>
    [Ignore]
    public TimeSpan? Duration { get; set; }
    [Ignore]
    public double TradesPerMonth => Duration.HasValue ? (TotalTrades * 30.0 / Duration.Value.TotalDays) : double.NaN;

    public double MaxBalanceDrawdown { get; set; }
    public double MaxBalanceDrawdownPerunum { get; set; }
    public double MaxEquityDrawdown { get; set; }
    public double MaxEquityDrawdownPerunum { get; set; }

    // TODO:
    // - MDWT Mean Days per Winning Trades
    // - Sortino Ratio?

    //[Ignore]
    //public List<object?>? PMultiSim { get; set; }
    public IPBot2? Parameters { get; set; }

    public bool IsAborted { get; set; }

    [Ignore]
    public IEnumerable<object>? JournalEntries { get; set; }
}

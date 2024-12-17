public class BacktestBatchJournalEntry
{
    public int BatchId { get; set; }
    public long Id { get; set; }
    public string StringId => $"{BatchId}-{Id}";

    public double Fitness { get; set; }
    public double AD { get; set; }

    /// <summary>
    /// Average minutes per winning trade
    /// </summary>
    public double AMWT { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Breakevens { get; set; }
    public int UnknownTrades { get; set; }

    public double MaxBalanceDrawdown { get; set; }
    public double MaxBalanceDrawdownPerunum { get; set; }
    public double MaxEquityDrawdown { get; set; }
    public double MaxEquityDrawdownPerunum { get; set; }

    // TODO:
    // - MDWT Mean Days per Winning Trades
    // - Sortino Ratio?

    //[Ignore]
    //public List<object?>? Parameters { get; set; }
    public object? Parameters { get; set; }

}

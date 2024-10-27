public class BacktestBatchJournalEntry
{
    public long Id { get; set; }

    public double Fitness { get; set; }
    public double AD { get; set; }

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

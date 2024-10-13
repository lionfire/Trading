public class BacktestBatchJournalEntry
{
    public long Id { get; set; }

    public double Fitness { get; set; }
    public double AD { get; set; }

    //[Ignore]
    //public List<object?>? Parameters { get; set; }
    public object? Parameters { get; set; }

}

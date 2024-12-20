namespace LionFire.Trading.Journal;

public class JournalStats
{
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public int BreakevenTrades { get; set; }
    public int UnknownTrades { get; set; }
    public double AverageMinutesPerWinningTrade { get; set; }
}

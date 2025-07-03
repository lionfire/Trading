using LionFire.Trading.Journal;
using LionFire.Trading.Maths;

namespace LionFire.Trading.Automation.Journaling.Trades;

public sealed partial class BotTradeJournal<TPrecision> where TPrecision : struct, INumber<TPrecision>
{
    public class JournalStatsCollector
    {
        public readonly RollingAverage AverageMinutesPerWinningTrade = new();

        public JournalStats JournalStats
        {
            get
            {
                s.AverageMinutesPerWinningTrade = AverageMinutesPerWinningTrade.CurrentAverage;
                return s;
            }
        }
        private JournalStats s = new();

        internal void OnClose<TPrecision>(JournalEntry<TPrecision> entry) where TPrecision : struct, INumber<TPrecision>
        {
            if (entry.RealizedGrossProfitDelta > TPrecision.Zero)
            {
                if (entry.Position == null) { throw new ArgumentNullException(nameof(entry.Position)); }
                var positionDuration = entry.Time - entry.Position.EntryTime; /// REVIEW - move this somewhere?
                AverageMinutesPerWinningTrade.AddValue(positionDuration.TotalMinutes);

                s.WinningTrades++;
            }
            else if (entry.RealizedGrossProfitDelta == TPrecision.Zero)
            {
                s.BreakevenTrades++;
            }
            else if (entry.RealizedGrossProfitDelta <= TPrecision.Zero)
            {
                s.LosingTrades++;
            }
            else { s.UnknownTrades++; }
        }
    }

}

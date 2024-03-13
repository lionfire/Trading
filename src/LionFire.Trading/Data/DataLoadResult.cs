using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public class DataLoadResult
    {
        public DataLoadResult(MarketSeriesBase series)
        {
            this.MarketSeriesBase = series;
        }

        public MarketSeriesBase MarketSeriesBase { get; set; }
        public MarketSeries MarketSeries { get { return MarketSeriesBase as MarketSeries; } }
        public MarketTickSeries MarketTickSeries { get { return MarketSeriesBase as MarketTickSeries; } }

        public DateTimeOffset StartDate;
        public DateTimeOffset EndDate;
        public DateTimeOffset QueryDate;
        public bool IsAvailable { get; set; }
        public bool IsPartial { get; set; }

        public List<TimedBar> Bars { get; set; }
        public List<Tick> Ticks { get; set; }

        public int Count
        {
            get { if (Bars != null) return Bars.Count; if (Ticks != null) return Ticks.Count; return 0; }
        }

        public static DataLoadResult AlreadyLoaded { get { return alreadyLoaded; } }

        public bool Faulted { get; set; }

        private static DataLoadResult alreadyLoaded = new DataLoadResult(null)
        {
            IsAvailable = true, IsPartial  = false,
        };

        public override string ToString()
        {
            var na = IsAvailable ? "" : " (Not available)";
            var p = IsPartial ? " (Partial)" : "";
            var s = StartDate.ToDefaultString();
            var e = EndDate.ToDefaultString();
            return $"{s} - {e}{na}{p}";
        }
    }
}

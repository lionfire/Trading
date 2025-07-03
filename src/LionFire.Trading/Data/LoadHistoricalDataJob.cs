using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{
    public abstract class LoadHistoricalDataJob : ProgressiveJob, IJob
    {

        #region Misc

        public override string ToString()
        {
            
            if (TimeFrame.Name == "m1")
            {
                var date = Date.ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                var date = Date.ToString("yyyy-MM-dd");
            }
            return $"{Symbol}-{TimeFrame.Name}: download data for {Date}";
        }

        #endregion


        #region Derived

        public string Symbol => MarketSeriesBase?.SymbolCode;
        public TimeFrame TimeFrame => MarketSeriesBase?.TimeFrame;

        public virtual IAccount_Old Account => MarketSeriesBase?.Account;
        public virtual IFeed_Old Feed => MarketSeriesBase?.Feed;

        public MarketSeries MarketSeries { get { return MarketSeriesBase as MarketSeries; } }
        public MarketTickSeries MarketTickSeries { get { return MarketSeriesBase as MarketTickSeries; } }

        #endregion

        public MarketSeriesBase MarketSeriesBase
        {
            get { return marketSeriesBase; }
            set { marketSeriesBase = value; }
        }
        private MarketSeriesBase marketSeriesBase;
        
        public DateTime Date { get; set; }
        public bool CacheOnly { get; set; } = false;
        public bool WriteCache { get; set; } = false;
        public TimeSpan? MaxOutOfDate { get; set; }

        public bool Faulted { get; set; }

        public LoadHistoricalDataJob() {
            CreateDate = DateTime.UtcNow;
        }
        public LoadHistoricalDataJob(MarketSeriesBase series) : this()
        {
            if (series == null) throw new ArgumentNullException(nameof(series));
            this.MarketSeriesBase = series;
            
        }


        public DateTime CreateDate { get; set; }
    }
}

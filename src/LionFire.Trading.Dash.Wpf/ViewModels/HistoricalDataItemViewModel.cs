using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{
    public class HistoricalDataItemViewModel
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool IsPartial { get; set; }

        public bool IsAvailable { get; set; }

        /// <summary>
        /// Bar/tick count
        /// </summary>
        public int Count { get; set; }

        public bool CanLoad()
        {
            return IsAvailable;
        }

        public void Load()
        {
        }

        //public static HistoricalDataItemViewModel Get(string symbolCode, TimeFrame timeFrame, DateTime chunkDateTime, IAccount account)
        //{
        //    var series = account.Data.GetMarketSeries(symbolCode, timeFrame);
        //    if (series == null)
        //    {
        //        return NotAvailable;
        //    }

        //    //account.HistoricalDataProvider.Load(symbolCode, timeFrame, chunkDateTime)
        //    //series.Get


        //}
        public static HistoricalDataItemViewModel NotAvailable = new HistoricalDataItemViewModel
        {
            IsAvailable = false
        };
    }

}

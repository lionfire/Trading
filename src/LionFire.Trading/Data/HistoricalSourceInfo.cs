using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.DataSources
{
    public class HistoricalSourceInfo
    {
        public string Symbol;

        /// <summary>
        /// BID or ASK
        /// </summary>
        public string PriceType;

        /// <summary>
        /// CANDLESTICK (OLHCV)
        /// </summary>
        public string DataType;

        public DateTime StartDate;
        public DateTime EndDate;

        public string TimeFrame;

        /// <summary>
        /// CSV, etc.
        /// </summary>
        public string Format;

        public bool TryParseFromFileName(string fileNameWithExtension)
        {
            return false;
        }

    }
}

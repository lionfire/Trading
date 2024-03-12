using System;
using System.Collections.Generic;
using System.Linq;
#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
using DataSeriesType = cAlgo.API.DataSeries;
#else
using DataSeriesType = LionFire.Trading.DataSeries;
#endif
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class MarketSeriesExtensions
    {
        public static DataSeriesType GetDataSeries(this MarketSeries marketSeries, OhlcAspect source)
        {
            switch (source)
            {
                case OhlcAspect.Open:
                    return marketSeries.Open;
                case OhlcAspect.High:
                    return marketSeries.High;
                case OhlcAspect.Low:
                    return marketSeries.Low;
                case OhlcAspect.Close:
                    return marketSeries.Close;
                default:
                    throw new ArgumentException();
            }
        }
    }
}

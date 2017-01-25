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
        public static DataSeriesType GetDataSeries(this MarketSeries marketSeries, BarComponent source)
        {
            switch (source)
            {
                case BarComponent.Open:
                    return marketSeries.Open;
                case BarComponent.High:
                    return marketSeries.High;
                case BarComponent.Low:
                    return marketSeries.Low;
                case BarComponent.Close:
                    return marketSeries.Close;
                default:
                    throw new ArgumentException();
            }
        }
    }
}

using System;
using System.Collections.Generic;
#if cTrader
using cAlgo.API.Internals;
#endif
using System.Text;

namespace LionFire.Trading
{
    
    public static class DataSeriesExtensions
    {
        public static double MinimumOffset(this MarketSeries series, int periods, int offset = 2)
        {
            var s = series.Low;
            double min = double.MaxValue;
            for (int i = offset; i < periods; i++)
            {
                var val = s.Last(i);
                if (val < min)
                {
                    min = val;
                }
            }
            return min;
        }
        public static double MaximumOffset(this MarketSeries series, int periods, int offset = 2)
        {
            var s = series.High;
            double max = double.MinValue;
            for (int i = offset; i < periods; i++)
            {
                var val = s.Last(i);
                if (val > max)
                {
                    max = val;
                }
            }
            return max;
        }
    }
}

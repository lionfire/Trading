#if cAlgo
using cAlgo.API;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    // REVIEW: Use Algo.CreateDataSeries instead?
    public class CustomIndicatorDataSeries : IndicatorDataSeries
    {
        List<double> values = new List<double>();

        public double this[int index] {
            get {
                if (index >= values.Count()) return double.NaN;
                return values[index];
            }

            set {
                while (index >= values.Count())
                {
                    if (index == values.Count())
                    {
                        values.Add(value);
                    }
                    else
                    {
                        values.Add(double.NaN);
                    }
                }
            }
        }

#if cAlgo
        double DataSeries.this[int index] {
            get {
                if (index >= values.Count()) return double.NaN;
                return values[index];
            }
        }
#endif

        public int Count {
            get {
                return values.Count;
            }
        }

        public double LastValue {
            get {
                if (values.Count == 0) return double.NaN;
                return values[values.Count - 1];
            }
        }

        public double Last(int index)
        {
            var x = values.Count - index;
            if (x < 0) return double.NaN;
            return values[x];
        }
    }
}

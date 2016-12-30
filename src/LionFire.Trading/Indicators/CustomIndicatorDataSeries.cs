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
        public double UnsetValue { get { return double.NaN; } }

        List<double> values = new List<double>();

        public double this[int index] {
            get {
                if (LastIndex == int.MinValue|| index >= LastIndex) return double.NaN;
                return values[index];
            }

            set {
                if (index < values.Count)
                {
                    values[index] = value;
                }
                else
                {
                    while (index >= values.Count)
                    {
                        if (index == values.Count())
                        {
                            values.Add(value);
                            return;
                        }
                        else
                        {
                            values.Add(double.NaN);
                        }
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
        public int LastIndex { get { if (values.Count > 0) return values.Count - 1; return int.MinValue; } }

        public double LastValue {
            get {
                if (LastIndex == int.MinValue) return double.NaN;
                return values[LastIndex];
            }
        }

        public double Last(int index)
        {
            var x = Count - index;
            if (x < 0) return double.NaN;
            return values[x];
        }
    }
}

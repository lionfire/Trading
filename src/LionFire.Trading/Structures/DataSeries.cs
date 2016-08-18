using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public sealed class BarSeries : DataSeries<TimedBarStruct>, IBarSeries
    {
    }

    public sealed class DoubleDataSeries : DataSeries<double>, IDataSeries
    {
    }

    public sealed class TimeSeries : DataSeries<DateTime>, ITimeSeries
    {
        public int FindIndex(DateTime time)
        {
            return list.FindLastIndex(d => d == time);
        }
    }

    public class DataSeries<ListType>
        where ListType : new()
    {
        #region Static

        static DataSeries()
        {
            DataSeries<double>.InvalidValue = double.NaN;
        }

        public static ListType InvalidValue;

        #endregion

        protected List<ListType> list = new List<ListType>();

        public ListType this[int index] {
            get {
                if (index >= list.Count) return InvalidValue;
                return list[index];
            }
            set {
                if (index >= list.Count)
                {
                    var padCount = index - list.Count;
                    if (padCount > 0)
                    {
                        list.AddRange(Enumerable.Repeat(InvalidValue, padCount));
                    }
                    list.Add(value);
                }
            }
        }

        public int Count { get { return list.Count; } }

        public ListType LastValue {
            get {
                if (list.Count == 0) return InvalidValue;
                return list[list.Count - 1];
            }
            internal set {
                if (list.Count == 0) throw new ArgumentOutOfRangeException("Cannot set LastValue because no values exist.");
                list[list.Count - 1] = value;
            }
        }

        public ListType Last(int indexFromEnd)
        {
            var absoluteIndex = (list.Count - 1) - indexFromEnd;
            if (absoluteIndex < 0) return InvalidValue;
            return list[absoluteIndex];
        }

        public void Add()
        {
            list.Add(InvalidValue);
        }

        public void Add(ListType[] items)
        {
            list.AddRange(items);
        }

        public void Add(ListType item)
        {
            list.Add(item);
        }
    }
}

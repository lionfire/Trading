﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public sealed class TimeSeries : DataSeries<DateTime>, ITimeSeries
    {
        public DateTime UnsetValue { get { return default(DateTime); } }

        public int FindIndex(DateTime time)
        {
            var result = list.FindLastIndex(d => d <= time);
            if (result == -1)
            {
                throw new NotImplementedException("needs reverseList");
                //result = reverseList.FindLastIndex(d => d <= time);
                //if (result == -1) return int.MinValue;
                //result = -1 - result;

            }
            return result;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class DataSeries<ListType>
        where ListType : new()
    {
        #region Static

        static DataSeries()
        {
            DataSeries<double>.InvalidValue = double.NaN;
        }

        public static ListType InvalidValue = default(ListType);

        #endregion

        protected List<ListType> list = new List<ListType>();

        public ListType this[int index]
        {
            get
            {
                if (index < 0)
                {
                    var reverseIndex = -index - 1;
                    if (reverseIndex >= reverseList.Count) return InvalidValue;
                    return reverseList[reverseIndex];
                }
                else
                {
                    if (index >= list.Count) return InvalidValue;
                    return list[index];
                }

            }
            set
            {
                if (index < 0)
                {
                    index = -index - 1;
                    SetListValue(reverseList, index, value);
                }
                else
                {
                    SetListValue(list, index, value);
                }
            }
        }

        private static void SetListValue(List<ListType> listParameter, int index, ListType val)
        {
            if (index > listParameter.Count)
            {
                var padCount = index - listParameter.Count;
                if (padCount > 0)
                {
                    listParameter.AddRange(Enumerable.Repeat(InvalidValue, padCount));
                }
                listParameter.Add(val);
            }
            else if (index == listParameter.Count)
            {
                listParameter.Add(val);
            }
            else
            {

                if (!(listParameter[index] is double && double.IsNaN((double)(object)listParameter[index]))
                    && !listParameter[index].Equals(val))
                {                    
                    Debug.WriteLine($"WARNING - resetting index {index} from {listParameter[index]} to {val}");
                    listParameter[index] = val;
                }
            }
        }

        public int Count { get { return list.Count + reverseList.Count; } }

        public ListType LastValue
        {
            get
            {
                return this[LastIndex];
            }
            internal set
            {
                if (list.Count == 0) throw new ArgumentOutOfRangeException("Cannot set LastValue because no values exist.");
                list[list.Count - 1] = value;
            }
        }

        /// <summary>
        /// Returns int.MinValue if no items
        /// </summary>
        public int LastIndex
        {
            get
            {
                if (list.Count != 0)
                {
                    return list.Count - 1;
                }
                else if (reverseList.Count > 0)
                {
                    return -1;
                }
                else
                {
                    return int.MinValue;
                }
            }
        }

        public ListType First(int indexFromStart = 0)
        {
            return this[FirstIndex + indexFromStart];
        }

        public ListType Last(int indexFromEnd = 0)
        {
            return this[LastIndex - indexFromEnd];
        }

        #region Add

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

        #endregion

        public int FirstIndex
        {
            get
            {
                if (reverseList != null && reverseList.Count > 0)
                {
                    return -reverseList.Count;
                }
                else if (list.Count > 0)
                {
                    return 0;
                }
                else
                {
                    return int.MaxValue;
                }
            }
        }

        #region Reverse List



        protected List<ListType> reverseList = new List<ListType>();

        public void AddReverse()
        {
            reverseList.Add(InvalidValue);
        }
        public void AddReverse(ListType[] items)
        {
            reverseList.AddRange(items);
        }
        public void AddReverse(ListType item)
        {
            reverseList.Add(item);
        }

        #endregion


    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading;

/// <summary>
/// A data series that supports a cursor, for backtesting scenarios where precomputed data may be loaded.
/// </summary>
/// <typeparam name="ListType"></typeparam>
public class CursorDataSeries<ListType>
    where ListType : new()
{
    public static ListType InvalidValue => DataSeries<ListType>.InvalidValue;

    public CursorDataSeries() { list = new List<ListType>(); }
    public CursorDataSeries(int capacity) { list = new List<ListType>(capacity); }


    public int Cursor { get; set; } = -1;
    protected List<ListType> list;

    public ListType this[int index]
    {
        get
        {
#if DEBUG || Strict
            if (Cursor < 0) { throw new ArgumentException($"{nameof(Cursor)} is not set"); }
            if (Cursor - index < 0) { throw new ArgumentException($"{nameof(Cursor)} - {nameof(index)} must be >= 0"); }
            if (Cursor - index >= list.Count) return InvalidValue;
#endif
            return list[Cursor - index];
        }
        set
        {
#if DEBUG || Strict
            if (Cursor < 0) { throw new ArgumentException($"{nameof(Cursor)} is not set"); }
            if (Cursor - index < 0) { throw new ArgumentException($"{nameof(Cursor)} - {nameof(index)} must be >= 0"); }
#endif

            var effectiveIndex = Cursor - index;
            SetRawIndex(Cursor - index, value);
            //if (Cursor - index >= list.Count) return InvalidValue;


        }
    }
    public void SetRawIndex(int index, ListType value)
    {
        if (index > list.Count)
        {
            var padCount = index - list.Count;
            if (padCount > 0)
            {
                list.AddRange(Enumerable.Repeat(InvalidValue, padCount));
            }
            list.Add(value);
        }
        else if (index == list.Count)
        {
            list.Add(value);
        }
        else
        {
            if (!(list[index] is double d && double.IsNaN(d) || list[index].Equals(value)))
            {
                Debug.WriteLine($"WARNING - changing non-default value at index {index} from {list[index]} to {value}");
            }
            list[index] = value;
        }
    }
}

public class DataSeries<ListType>
    where ListType : new()
{
    #region Static

    static DataSeries()
    {
        DataSeries<double>.InvalidValue = double.NaN;
        DataSeries<float>.InvalidValue = float.NaN;
    }

    public static ListType InvalidValue = default(ListType);

    #endregion

    protected List<ListType> list;

    public DataSeries() { list = new List<ListType>(); }
    public DataSeries(int capacity) { list = new List<ListType>(capacity); }

    public ListType this[int index]
    {
        get
        {
            if (index < 0) { throw new ArgumentException($"{nameof(index)} must be >= 0"); }

            if (index >= list.Count) return InvalidValue;
            return list[index];

        }
        set
        {
            if (index < 0)
            {
                throw new ArgumentException($"{nameof(index)} must be >= 0");
                //index = -index - 1;
                //SetListValue(reverseList, index, value);
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

    public int Count { get { return list.Count; } }

    /// <summary>
    /// Returns int.MaxValue if there is no data
    /// </summary>
    public int FirstIndex => list.Count > 0 ? 0 : int.MaxValue;

    public ListType LastValue
    {
        get => this[LastIndex];
        internal set
        {
            if (list.Count == 0) throw new ArgumentOutOfRangeException("Cannot set LastValue because no values exist.");
            list[list.Count - 1] = value;
        }
    }

    /// <summary>
    /// Returns int.MinValue if no items
    /// </summary>
    public int LastIndex => list.Count != 0 ? list.Count - 1 : int.MinValue;

    public ListType First(int indexFromStart = 0) => this[FirstIndex + indexFromStart];

    public ListType Last(int indexFromEnd = 0) => this[LastIndex - indexFromEnd];

    #region Add

    public void Add() => list.Add(InvalidValue);
    public void Add(ListType[] items) => list.AddRange(items);
    public void Add(ListType item) => list.Add(item);

    #endregion

}


#if REVIEW
/// <summary>
/// Implemented using a backward list and a forward list
/// TODO EXPERIMENTAL REVIEW - is there really a good purpose/need for this?
/// </summary>
/// <typeparam name="ListType"></typeparam>
public class BidirectionalDataSeries<ListType>
    where ListType : new()
{
#region Static

    public static ListType InvalidValue => DataSeries<ListType>.InvalidValue;

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

            if (index >= list.Count) return InvalidValue;
            return list[index];

        }
        set
        {
            if (index < 0)
            {
                throw new ArgumentException($"{nameof(index)} must be >= 0");
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

    /// <summary>
    /// Returns int.MaxValue if there is no data
    /// </summary>
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
            //else if (reverseList.Count > 0)
            //{
            //    return -1;
            //}
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
#endif

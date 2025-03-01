using System.Text.Json.Serialization;

namespace LionFire.Trading.Structures;

//public class HistogramBucket
//{
//    public bool AndBeyond { get; set; }
//    public double Upper { get; set; }
//    public double Lower { get; set; }
//    public bool IsUpperExclusive => Upper > 0;
//    public bool IsLowerExclusive => Lower < 0;

//    public int Count { get; set; }

//}
public class Histogram
{
    #region Parameters

    [JsonInclude]
    public double BucketSize { get; }
    [JsonInclude]
    public double Min { get; }
    [JsonInclude]
    public double Max { get; }

    #endregion

    #region Cache

    [JsonIgnore]
    private int FirstPositiveBucketIndex => firstPositiveBucketIndex ??= (int)Math.Ceiling(-Min / BucketSize);
    private int? firstPositiveBucketIndex;

    #endregion

    public Histogram() { }
    public Histogram(double bucketSize, double min, double max, IEnumerable<double>? values = null)
    {
        if (bucketSize <= 0) { throw new ArgumentException($"{nameof(bucketSize)} must be greater than zero"); }

        if (min > 0 || max < 0 || min == 0 && max == 0) { throw new NotSupportedException($"{nameof(min)} must be 0 or lower and {nameof(max)} must be zero or greater and they cannot both be zero"); }

        BucketSize = bucketSize;
        Min = min;
        Max = max;
        var bucketCount = (int)(Math.Ceiling(max / bucketSize) + Math.Max(1, Math.Ceiling(-min / bucketSize)));
        buckets = new List<int>(bucketCount);
        for (int i = 0; i < bucketCount; i++) { buckets.Add(0); }



        if (values != null) { AddData(values); }
    }

    [JsonInclude]
    public List<int> Buckets { get => buckets; protected set => buckets = value; }
    private List<int> buckets;

    public int GetBucketForValue(double value)
    {
        if (value > 0)
        {
            return Math.Min(buckets.Count - 1, FirstPositiveBucketIndex + (int)Math.Floor(value / BucketSize));
        }
        else // <= 0
        {
            return Math.Max(0, FirstPositiveBucketIndex - 1 - (value == 0 ? 0 : (int)Math.Floor(value / BucketSize)));
        }
    }

    public void AddData(IEnumerable<double> values)
    {
        foreach (var value in values)
        {
            buckets[GetBucketForValue(value)]++;
        }
    }

    #region Query

    public int AtLeast(double min)
    {
        int sum = 0;
        for (int i = GetBucketForValue(min); i < buckets.Count; i++)
        {
            sum += buckets[i];
        }
        return sum;
    }

    #endregion
}
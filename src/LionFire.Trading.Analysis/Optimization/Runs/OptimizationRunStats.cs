using LinqStatistics;
//using LinqStatistics.NaN;
using LionFire.Trading.Structures;
//using MathNet.Numerics.Statistics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using static LionFire.Reflection.GetMethodEx;

namespace LionFire.Trading.Automation.Optimization;

public class OptimizationRunStats
{
    public const int CurrentVersion = 3;

    //public OptimizationRunStats() { }

    public int Version { get; set; }
    public int BacktestsCount { get; set; }

    public NumericStats<double> AD { get; set; }
    public NumericStats<double> TradesPerMonth { get; set; }
    public NumericStats<int> TotalTrades { get; set; }
    public NumericStats<double> WinRate { get; set; }
    public NumericStats<double?> SortinoRatio { get; set; }
    public NumericStats<double?> SharpeRatio { get; set; }
    public NumericStats<double?> ProfitFactor { get; set; }
    public NumericStats<double> Aroi { get; set; }
    public NumericStats<double> AverageDaysPerWinningTrade { get; set; }
    public NumericStats<double> AverageDaysPerLosingTrade { get; set; }

    public NumericStats<double> AverageDaysPerTrade { get; set; }
    public NumericStats<double> AverageTrade { get; set; }
    public NumericStats<double> AverageTradePerVolume { get; set; }
    public NumericStats<double> Fitness { get; set; }


    [JsonIgnore]
    public IEnumerable<NumericStats<double>> All
    {
        get
        {
            yield return AD;
            yield return Aroi;
            yield return TradesPerMonth;
            yield return WinRate;
            yield return AverageDaysPerWinningTrade;
            yield return AverageDaysPerLosingTrade;
            yield return AverageDaysPerTrade;
            yield return AverageTrade;
            yield return Fitness;
        }
    }

    [JsonIgnore]
    public IQueryable<NumericStats<double>> AllQueryable => All.AsQueryable();

    public DateTime GeneratedOn { get; set; }
    public Histogram ADHistogram { get; set; }
    public Histogram NADHistogram { get; set; }
    public Histogram PADHistogram { get; set; }
    public Histogram DADHistogram { get; set; }
    public Histogram AADHistogram { get; set; }


    static int DadThreshold = 60;
    public double DadScore
    {
        get
        {
            if (double.IsNaN(dadScore) && DADHistogram != null)
            {
                dadScore = Math.Min(DadThreshold, DADHistogram.AtLeast(1)) / (double)DadThreshold;
            }
            return dadScore;
        }
    }
    private double dadScore = double.NaN;

    static int NadThreshold = 30;
    public double NadScore
    {
        get
        {
            if (double.IsNaN(nadScore) && NADHistogram != null)
            {
                nadScore = Math.Min(NadThreshold, NADHistogram.AtLeast(1)) / (double)NadThreshold;
            }
            return nadScore;
        }
    }
    private double nadScore = double.NaN;

    public double Score { get; set; } = double.NaN;

}

public class NumericStats<T> : IEquatable<T>
//where T : NumericStats
{
    public string? Name { get; set; }
    public T? Max { get; set; }
    public T? Min { get; set; }
    public double? Mean { get; set; }
    public double? Variance { get; set; }
    public double? StandardDeviation { get; set; }

    public override bool Equals(object? other)
    {
        var ns = other as NumericStats<T>;
        if (ns == null) return false;
        return ns.Name == Name;
    }
    public bool Equals(T other)
    {
        var ns = other as NumericStats<T>;
        if (ns == null) return false;
        return ns.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }
}

public static class NumericStatsExtensions
{
    public static NumericStats<double?> GetStats(this IEnumerable<double?> numbers, string name)
    {
        var result = new NumericStats<double?>();

        int count = numbers.Count();
        result.Name = name;
        result.Max = numbers.Max();
        result.Min = numbers.Min();
        result.Mean = numbers.AverageNaN();
        if (count > 1)
        {
            result.Variance = numbers.Variance();
            result.StandardDeviation = numbers.StandardDeviation();
        }
        return result;
    }
    public static NumericStats<double> GetStats(this IEnumerable<double> numbers, string name)
    {
        var result = new NumericStats<double>();

        int count = numbers.Count();
        result.Name = name;
        if (count > 0)
        {
            result.Max = numbers.Max();
            result.Min = numbers.Min();
            result.Mean = numbers.AverageNaN();
        }
        if (count > 1)
        {
            result.Variance = numbers.Variance();
            result.StandardDeviation = numbers.StandardDeviation();
        }
        return result;
    }
    public static NumericStats<int?> GetStats(this IEnumerable<int?> numbers, string name)
    {
        var result = new NumericStats<int?>();

        int count = numbers.Count();
        result.Name = name;
        result.Max = numbers.Max();
        result.Min = numbers.Min();
        result.Mean = numbers.AverageNaN();
        if (count > 1)
        {
            result.Variance = numbers.Variance();
            result.StandardDeviation = numbers.StandardDeviation();
        }
        return result;
    }
    public static NumericStats<int> GetStats(this IEnumerable<int> numbers, string name)
    {
        var result = new NumericStats<int>();

        int count = numbers.Count();
        result.Name = name;
        if (count > 0)
        {
            result.Max = numbers.Max();
            result.Min = numbers.Min();
            result.Mean = numbers.AverageNaN();
        }
        if (count > 1)
        {
            result.Variance = numbers.Variance();
            result.StandardDeviation = numbers.StandardDeviation();
        }
        return result;
    }
}

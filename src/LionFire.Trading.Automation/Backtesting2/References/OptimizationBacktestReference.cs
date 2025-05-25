using LionFire.Serialization;
using System.ComponentModel;

namespace LionFire.Trading.Automation;

[TypeConverter(typeof(ParsableConverter<OptimizationBacktestReference>))]
public class OptimizationBacktestReference : IParsableSlim<OptimizationBacktestReference>
{
    public OptimizationRunReference? OptimizationRunReference { get; set; }
    public const char Separator = ';';

    public int BatchId { get; set; }
    public long BacktestId { get; set; }

    #region Serialization

    public static OptimizationBacktestReference Parse(string s)
    {
        var chunks = s.Split(Separator);
        var chunks2 = chunks[1].Split(BacktestReference.Separator);

        return new OptimizationBacktestReference
        {
            OptimizationRunReference = OptimizationRunReference.Parse(chunks[0]),
            BatchId = int.Parse(chunks2[0]),
            BacktestId = int.Parse(chunks2[1]),
        };
    }
    public override string ToString() =>
        $"{OptimizationRunReference?.ToString()}{Separator}{BatchId}{BacktestReference.Separator}{BacktestId}";

    #endregion

}

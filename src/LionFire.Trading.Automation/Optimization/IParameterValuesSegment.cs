
using LionFire.Trading.Backtesting2;
using LionFire.Trading.Optimizing2;

namespace LionFire.Trading.Automation.Optimization;

public interface IParameterValuesSegment
{
    object CurrentValue { get; }
    bool IsFinished { get; }
    //IPParameterOptimization ParameterRange { get; }
    //long MaxPosition { get; set; }
    //long Position { get; set; }
    HierarchicalPropertyInfo Info { get; }
    void MoveNext();
}


public class ParameterValuesSegment<T> : IParameterValuesSegment
    where T : INumber<T>
{
    public PParameterOptimization<T> ParameterRange { get; }
    public HierarchicalPropertyInfo Info { get; }

    public ParameterValuesSegment(PParameterOptimization<T> parameterRange, HierarchicalPropertyInfo info)
    {
        ParameterRange = parameterRange;
        Info = info;
        Position = ParameterRange.Min;
        MaxPosition = ParameterRange.Max;
    }

    public void MoveNext()
    {
        if (IsFinished) return;
        Position = Position + ParameterRange.Step;
        if (Position.CompareTo(ParameterRange.Max) > 0)
        {
            Position = ParameterRange.Max;
        }
        //currentValue = Position;
    }

    public bool IsFinished => Position >= MaxPosition;
    public T Position { get; set; } 
    public T MaxPosition { get; set; }

    public object CurrentValue => Position;
    //private T currentValue;

}


public class OptimizationLevelOfDetail
{
    public int Level { get; set; }

    public OptimizationLevelOfDetail(int level, POptimization pOptimization, OptimizationLevelOfDetail? previousLevel)
    {
        Level = level;

        //if (previousLevel != null)
        //{
        //}

        List<IParameterValuesSegment> segments = new();

        #region ranges

        //foreach (var parameterRange in PMultiSim.ParameterRanges)
        //{
        //    var info = botParameters.PathDictionary.TryGetValue(parameterRange.Name)
        //        ?? botParameters.NameDictionary.TryGetValue(parameterRange.Name)
        //        ?? throw new ArgumentException($"Parameter {parameterRange.Name} not found in {PMultiSim.BotParametersType}");

        //    var range = parameterRange.Create(info);
        //    segments.Add(range);
        //}

        #endregion

     
    }
}
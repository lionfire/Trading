using LionFire.Trading.Data;
using LionFire.Trading.IO;

namespace LionFire.Trading.Indicators;

public interface IIndicator2
{
    uint MaxLookback { get; }
    uint? DefaultMaxFastForwardBars => 5;
}


public interface IIndicator2<TIndicator, TParameters, TInput, TOutput>
    : IIndicator2<TParameters, TInput, TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
{
    static abstract TIndicator Create(TParameters p);
}

public interface IIndicator2<TParameters, TInput, TOutput>
: IIndicator2
, IObservable<IReadOnlyList<TOutput>>
, IObserver<IReadOnlyList<TInput>>
{
    //static abstract IndicatorMachine CreateMachine();

    //static abstract TIndicator Create<TIndicator>(TParameters p)
    //where TIndicator : IIndicator<TParameters, TInput, TOutput>;

    //IReadOnlyList<TOutput> Compute(TParameters parameter, IReadOnlyList<TInput> values)
    //{
    //    new HistoricalIndicatorExecutorX<ITIndicator, >
    //}
    //static abstract IOComponent Characteristics(TParameters parameter);
    static abstract List<InputSlot> Inputs();
    static abstract List<OutputSlot> Outputs();

    void Clear();

    #region Input Handling

    // OPTIMIZE: try the return value as an array
    Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);

    void OnNextFromArray(IReadOnlyList<TInput> inputData, int index);

    #endregion

}

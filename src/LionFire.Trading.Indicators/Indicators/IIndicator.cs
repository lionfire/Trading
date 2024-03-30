using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harness;

namespace LionFire.Trading.Indicators;

public interface IIndicator
{
    uint Lookback { get; }
    uint? DefaultMaxFastForwardBars => 5;
}


public interface IIndicator<TIndicator, TParameters, TInput, TOutput>
    : IIndicator<TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{
    static abstract TIndicator Create(TParameters p);
}

public interface IIndicator<TParameters, TInput, TOutput>
: IIndicator
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
    static abstract IndicatorCharacteristics Characteristics(TParameters parameter);

    void Clear();

    #region Input Handling

    // OPTIMIZE: try the return value as an array
    Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);

    void OnNextFromArray(IReadOnlyList<TInput> inputData, int index);

    #endregion

}

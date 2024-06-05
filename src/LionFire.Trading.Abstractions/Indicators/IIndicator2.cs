using LionFire.Trading.Data;
using LionFire.Trading.IO;

namespace LionFire.Trading.Indicators;

public interface IIndicator2
{
    int MaxLookback { get; }
    uint? DefaultMaxFastForwardBars => 5;
    bool IsReady { get; }
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

    //static abstract TIndicator Create<TIndicator>(TValue p)
    //where TIndicator : IIndicator<TValue, InputSlot, TOutput>;

    //IReadOnlyList<TOutput> Compute(TValue parameter, IReadOnlyList<InputSlot> values)
    //{
    //    new HistoricalIndicatorExecutorX<ITIndicator, >
    //}
    //static abstract IOComponent Characteristics(TValue parameter);


    void Clear();

    #region Input Handling

    //void OnNextFromArray(IReadOnlyList<TInput> inputData, int index); // OLD - search code and remove all of these.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="outputBuffer"></param>
    /// <param name="outputIndex"></param>
    /// <param name="publishOutput"></param>
    /// <returns>Number of values written</returns>
    void OnNext(IReadOnlyList<TInput> input, TOutput[]? outputBuffer, int outputIndex = 0, int outputSkip = 0);

    #endregion

}

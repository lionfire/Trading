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

    //static abstract TIndicator Create<TIndicator>(TValue p)
    //where TIndicator : IIndicator<TValue, InputSlot, TOutput>;

    //IReadOnlyList<TOutput> Compute(TValue parameter, IReadOnlyList<InputSlot> values)
    //{
    //    new HistoricalIndicatorExecutorX<ITIndicator, >
    //}
    //static abstract IOComponent Characteristics(TValue parameter);
    static abstract IReadOnlyList<InputSlot> InputSlots();
    static abstract IReadOnlyList<OutputSlot> Outputs();

    void Clear();

    #region Input Handling

  

    void OnNextFromArray(IReadOnlyList<TInput> inputData, int index);

    #endregion

}

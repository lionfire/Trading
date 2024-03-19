namespace LionFire.Trading.Indicators;

public interface IIndicator
{
    uint Lookback { get; }
    uint? DefaultMaxFastForwardBars => 5;
}

public interface IIndicator<TParameters, TInput, TOutput> 
    : IIndicator
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
{
    //static abstract IndicatorMachine CreateMachine();

    static abstract IIndicator<TParameters, TInput, TOutput> Create(TParameters p);

    static abstract IReadOnlyList<TOutput> Compute(TParameters parameter, IReadOnlyList<TInput> values);
    static abstract IndicatorCharacteristics Characteristics(TParameters parameter);

    void Clear();
}

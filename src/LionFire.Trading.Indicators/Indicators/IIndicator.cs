namespace LionFire.Trading.Indicators;

public interface IIndicator
{
}

public interface IIndicator<TParameters, TInput, TOutput> 
    : IIndicator
    , IObservable<IEnumerable<TOutput>>
    , IObserver<IEnumerable<TInput>>
{
    //static abstract IndicatorMachine CreateMachine();

    static abstract IIndicator<TParameters, TInput, TOutput> Create(TParameters p);

    static abstract IEnumerable<TOutput> Compute(TParameters parameter, IEnumerable<TInput> values);
    static abstract IndicatorCharacteristics Characteristics(TParameters parameter);
}

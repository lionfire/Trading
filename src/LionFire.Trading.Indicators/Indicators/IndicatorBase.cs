using Baseline;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators;

public abstract class IndicatorBase<TConcrete, TParameters, TInput, TOutput>
    : IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    , IObserver<TInput>

    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator<TParameters, TInput, TOutput>
{

    #region (static) Implementation

    public static IEnumerable<TOutput> Compute(TParameters parameters, IEnumerable<TInput> inputs)
    {
        var indicator = TConcrete.Create(parameters);
        var result = new List<TOutput>();
        var d = indicator.Subscribe(result.AddRange);
        indicator.OnNext(inputs);
        d.Dispose();
        return result;
    }

    #endregion

    #region (static) Value Utilities

    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Parameters

    #region Derived

    public abstract uint Lookback { get; }
    
    #endregion

    #endregion

    #region State

    protected Subject<IReadOnlyList<TOutput>>? subject;

    #endregion

    #region IObservable

    public IDisposable Subscribe(IObserver<IReadOnlyList<TOutput>> observer)
    {
        subject ??= new();
        return subject.Subscribe(observer);
    }

    public abstract void OnNext(IReadOnlyList<TInput> value);
    public virtual void OnNext(TInput value) => OnNext([value]);

    public virtual void OnCompleted() { }

    public virtual void OnError(Exception error)
    {
        Debug.WriteLine($"{this.GetType().FullName} OnError: {error}");
    }

    #endregion

    #region Methods

    public virtual void Clear()
    {
        subject?.OnCompleted();
    }

    #endregion
}


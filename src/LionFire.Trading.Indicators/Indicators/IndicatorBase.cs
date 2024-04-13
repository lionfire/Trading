using Baseline;
using LionFire.Trading.Data;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators;

public abstract class IndicatorBase<TConcrete, TParameters, TInput, TOutput>
    : IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    , IObserver<TInput>

    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator2<TConcrete, TParameters, TInput, TOutput>
{

    #region (static) Implementation

    public static IReadOnlyList<TOutput> Compute(TParameters parameters, IReadOnlyList<TInput> inputs)
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

    public abstract uint MaxLookback { get; }
    
    #endregion

    #endregion

    #region State

    /// <summary>
    /// If true, do not omit output when new input is available
    /// </summary>
    public bool IsPaused { get; set; }

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
        subject = null;
    }

    #region Input Handling

    //public abstract Task<(object, int)> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);
    public abstract Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);

    //public abstract void OnNextFromArray(object inputData, int index);


    //public override async Task<(IReadOnlyList<TInput>, int)> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var d1 = GetData<TInput1>(sources[0], start, endExclusive);
    //    var d2 = GetData<TInput2>(sources[1], start, endExclusive);
    //    await Task.WhenAll(d1, d2);

    //    int count = d1.Result.count;
    //    if (count != d2.Result.count) throw new ArgumentException("Input data counts do not match");

    //    return (new object[] { d1.Result.data, d2.Result.data }, count);
    //}

    public void OnNextFromArray(IReadOnlyList<TInput> inputData, int index)
    {
        OnNext(inputData[index]);
    }

    #endregion

    //#region Input



    //public void OnNextFromPreparedSources(IReadOnlyList<IHistoricalTimeSeries> sources, int index)
    //{
    //    OnNext(InputFromSources(sources, index));
    //}

    //public Func<IReadOnlyList<IHistoricalTimeSeries>, int, TInput> TInput InputFromSources(IReadOnlyList<IHistoricalTimeSeries> s)
    //{
    //    var s = (IHistoricalTimeSeries<decimal>)s[0];

    //    return (TInput) s[0][index];
    //}

    //#endregion

    #endregion
}


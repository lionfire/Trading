using Baseline;
using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators;

// REVIEW - make more generic to a BatchMarketProcessor or BatchProcessor?

//public interface IBarProcessor
//{
//    void OnBar();
//}
public interface ITickProcessor
{
    void OnTick();
}

public abstract class BarProcessor<TInput>
{

    protected virtual void OnBar() => throw new NotImplementedException();
}

public abstract class BarBatchProcessor<TInput, TOutput> : BarProcessor<TInput>
{
    // ENH: consider replacing parameters with a struct for better DX
    public virtual void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            CurrentInput = inputs[i];
            OnBar();
            if (output != null)
            {
                if (!currentOutputWasSet) throw new InvalidOperationException($"{nameof(OnBar)} failed to assign a value to CurrentOutput");
                output[outputIndex++] = CurrentOutput;
                currentOutputWasSet = false;
            }
        }
    }

    TInput CurrentInput = default!;

    /// <summary>
    /// Value is only valid if currentOutputWasSet is true
    /// </summary>
    protected TOutput CurrentOutput
    {
        private get => currentOutput;
        set
        {
            currentOutput = value;
            currentOutputWasSet = true;
        }
    }
    TOutput currentOutput = default!;
    private bool currentOutputWasSet;
}

public abstract class IndicatorBase<TConcrete, TParameters, TInput, TOutput>
    : BarBatchProcessor<TInput, TOutput>
    //, IObservable<IReadOnlyList<TOutput>>
    //, IObserver<IReadOnlyList<TInput>>
    //, IObserver<TInput>
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

    public abstract int MaxLookback { get; }

    #endregion

    #endregion

    #region Lifecycle

    public IndicatorBase()
    {
        InitState();
    }

    #endregion

    #region State

    protected Subject<IReadOnlyList<TOutput>>? subject;

    //public uint InputsNeededToBecomeReady { get; set; }
    public abstract bool IsReady { get; }// => InputsNeededToBecomeReady > 0;

    #endregion

    #region IObservable

    public IDisposable Subscribe(IObserver<IReadOnlyList<TOutput>> observer)
    {
        subject ??= new();
        return subject.Subscribe(observer);
    }


    public virtual void OnNext(TInput value) => OnNext([value]);
    public void OnNext(IReadOnlyList<TInput> inputs)
    {
        TOutput[]? output;
        var s = subject;
        if (s != null && !s.HasObservers)
        {
            subject = null;
            s = null;
            output = null;
        }
        else
        {
            output = new TOutput[inputs.Count];
        }
        OnBarBatch(inputs, output, 0, 0);

        // OLD
        //foreach (var input in inputs)
        //{
        //    if (buffer.IsFull) { sum -= buffer.Back(); }
        //    sum += input;
        //    buffer.PushFront(input);
        //    if (output != null)
        //    {
        //        if (buffer.IsFull)
        //        {
        //            output.Add(sum / Period);
        //        }
        //        else
        //        {
        //            output.Add(double.NaN);
        //        }
        //    }
        //}

        if (s != null
            //&& output != null  // Redundant
            )
        {
#if DEBUG
            if (output!.Length != inputs.Count) { ThrowUnreachable(); }
#endif
            s.OnNext(output!);
        }
    }

    public static void ThrowUnreachable() => throw new UnreachableCodeException();



    public virtual void OnCompleted() { }

    public virtual void OnError(Exception error)
    {
        Debug.WriteLine($"{this.GetType().FullName} OnError: {error}");
    }

    #endregion

    #region Methods

    public void InitState()
    {
        //InputsNeededToBecomeReady = MaxLookback;
    }

    public virtual void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        InitState();
    }


    #region Input Handling

    //public void OnNextFromArray(IReadOnlyList<TInput> inputData, int index) => OnNext(inputData[index]);

    #endregion

    //#region InputSignal

    //public void OnNextFromPreparedSources(IReadOnlyList<IHistoricalTimeSeries> sources, int index)
    //{
    //    OnNext(InputFromSources(sources, index));
    //}

    //public Func<IReadOnlyList<IHistoricalTimeSeries>, int, InputSlot> InputSlot InputFromSources(IReadOnlyList<IHistoricalTimeSeries> s)
    //{
    //    var s = (IHistoricalTimeSeries<decimal>)s[0];

    //    return (InputSlot) s[0][index];
    //}

    //#endregion

    #endregion

    #region IHistoricalTimeSeries

    //public abstract ValueTask<HistoricalDataResult<TOutput>> Get(DateTimeOffset start, DateTimeOffset endExclusive);
    //public abstract TimeFrame TimeFrame { get; }
    //public Type ValueType => typeof(TOutput);

    #endregion
}


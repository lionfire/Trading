using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for OBV implementations with common logic
/// </summary>
public abstract class OBVBase<TInput, TOutput>
    : IOBV<TInput, TOutput>
    , IIndicator2<POBV<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected POBV<TInput, TOutput> Parameters { get; }

    public abstract TOutput CurrentValue { get; }
    
    public abstract TOutput LastChange { get; }
    
    public virtual bool IsRising => LastChange > TOutput.Zero;
    
    public virtual bool IsFalling => LastChange < TOutput.Zero;

    public int MaxLookback => 1; // OBV only needs current and previous values
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected OBVBase(POBV<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        // OBV has minimal parameter requirements
        // Main validation is on the input data having both price and volume components
    }

    #endregion
    
    #region IObservable Implementation
    
    public IDisposable Subscribe(IObserver<IReadOnlyList<TOutput>> observer)
    {
        subject ??= new();
        return subject.Subscribe(observer);
    }
    
    #endregion
    
    #region IObserver Implementation
    
    public void OnNext(IReadOnlyList<TInput> value)
    {
        TOutput[]? output = null;
        var s = subject;
        
        if (s != null && !s.HasObservers)
        {
            subject = null;
            s = null;
        }
        else if (s != null)
        {
            output = new TOutput[value.Count];
        }
        
        OnBarBatch(value, output, 0, 0);
        
        if (s != null && output != null)
        {
            s.OnNext(output);
        }
    }
    
    public void OnNext(TInput value) => OnNext(new[] { value });
    
    public virtual void OnCompleted() 
    { 
        subject?.OnCompleted();
    }
    
    public virtual void OnError(Exception error)
    {
        subject?.OnError(error);
    }
    
    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
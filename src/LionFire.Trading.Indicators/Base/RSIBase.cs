using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for RSI implementations with common logic
/// </summary>
public abstract class RSIBase<TInput, TOutput>
    : IRSI<TInput, TOutput>
    , IIndicator2<PRSI<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PRSI<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public TOutput OverboughtLevel => Parameters.OverboughtLevel;
    
    public TOutput OversoldLevel => Parameters.OversoldLevel;
    
    public abstract TOutput CurrentValue { get; }
    
    public virtual bool IsOverbought => IsReady && CurrentValue > OverboughtLevel;
    
    public virtual bool IsOversold => IsReady && CurrentValue < OversoldLevel;

    public int MaxLookback => Parameters.Period;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected RSIBase(PRSI<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("RSI period must be at least 2", nameof(Parameters.Period));
            
        if (Parameters.OverboughtLevel <= Parameters.OversoldLevel)
            throw new ArgumentException("Overbought level must be greater than oversold level");
            
        var hundred = TOutput.CreateChecked(100);
        var zero = TOutput.Zero;
        
        if (Parameters.OverboughtLevel > hundred || Parameters.OverboughtLevel < zero)
            throw new ArgumentException("Overbought level must be between 0 and 100");
            
        if (Parameters.OversoldLevel > hundred || Parameters.OversoldLevel < zero)
            throw new ArgumentException("Oversold level must be between 0 and 100");
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
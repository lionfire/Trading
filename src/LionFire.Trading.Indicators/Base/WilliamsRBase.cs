using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using LionFire.Trading;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Williams %R implementations with common logic
/// </summary>
public abstract class WilliamsRBase<TInput, TOutput>
    : IWilliamsR<TInput, TOutput>
    , IIndicator2<PWilliamsR<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PWilliamsR<TInput, TOutput> Parameters { get; }

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

    protected WilliamsRBase(PWilliamsR<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Williams %R period must be at least 2", nameof(Parameters.Period));
            
        if (Parameters.OverboughtLevel <= Parameters.OversoldLevel)
            throw new ArgumentException("Overbought level must be greater than oversold level");
            
        var zero = TOutput.Zero;
        var negHundred = TOutput.CreateChecked(-100);
        
        if (Parameters.OverboughtLevel > zero || Parameters.OverboughtLevel < negHundred)
            throw new ArgumentException("Overbought level must be between -100 and 0");
            
        if (Parameters.OversoldLevel > zero || Parameters.OversoldLevel < negHundred)
            throw new ArgumentException("Oversold level must be between -100 and 0");
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
    
    public void OnNext(IReadOnlyList<HLC<TInput>> value)
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
    
    public void OnNext(HLC<TInput> value) => OnNext(new[] { value });
    
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
    
    public abstract void OnBarBatch(IReadOnlyList<HLC<TInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
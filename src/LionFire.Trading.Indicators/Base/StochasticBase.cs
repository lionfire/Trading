using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Stochastic Oscillator implementations with common logic
/// </summary>
public abstract class StochasticBase<TInput, TOutput>
    : IStochastic<TInput, TOutput>
    , IIndicator2<PStochastic<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PStochastic<TInput, TOutput> Parameters { get; }

    public int FastPeriod => Parameters.FastPeriod;
    
    public int SlowKPeriod => Parameters.SlowKPeriod;
    
    public int SlowDPeriod => Parameters.SlowDPeriod;
    
    public TOutput OverboughtLevel => Parameters.OverboughtLevel;
    
    public TOutput OversoldLevel => Parameters.OversoldLevel;
    
    public abstract TOutput PercentK { get; }
    
    public abstract TOutput PercentD { get; }
    
    public virtual bool IsOverbought => IsReady && PercentK > OverboughtLevel;
    
    public virtual bool IsOversold => IsReady && PercentK < OversoldLevel;

    public int MaxLookback => Parameters.FastPeriod + Parameters.SlowKPeriod + Parameters.SlowDPeriod;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected StochasticBase(PStochastic<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.FastPeriod < 2)
            throw new ArgumentException("Fast period must be at least 2", nameof(Parameters.FastPeriod));
            
        if (Parameters.SlowKPeriod < 1)
            throw new ArgumentException("Slow K period must be at least 1", nameof(Parameters.SlowKPeriod));
            
        if (Parameters.SlowDPeriod < 1)
            throw new ArgumentException("Slow D period must be at least 1", nameof(Parameters.SlowDPeriod));
            
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
            // Stochastic has two outputs: %K and %D
            output = new TOutput[value.Count * 2];
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
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using LionFire.Trading;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Fibonacci Retracement implementations with common logic
/// </summary>
public abstract class FibonacciRetracementBase<TPrice, TOutput>
    : IFibonacciRetracement<HLC<TPrice>, TOutput>
    , IIndicator2<PFibonacciRetracement<HLC<TPrice>, TOutput>, HLC<TPrice>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TPrice>>>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PFibonacciRetracement<HLC<TPrice>, TOutput> Parameters { get; }

    public int LookbackPeriod => Parameters.LookbackPeriod;
    
    public abstract TOutput SwingHigh { get; }
    
    public abstract TOutput SwingLow { get; }
    
    public abstract TOutput Level000 { get; }
    
    public abstract TOutput Level236 { get; }
    
    public abstract TOutput Level382 { get; }
    
    public abstract TOutput Level500 { get; }
    
    public abstract TOutput Level618 { get; }
    
    public abstract TOutput Level786 { get; }
    
    public abstract TOutput Level1000 { get; }
    
    public abstract TOutput Level1618 { get; }
    
    public abstract TOutput Level2618 { get; }

    public int MaxLookback => Parameters.LookbackPeriod;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected FibonacciRetracementBase(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.LookbackPeriod < 10)
            throw new ArgumentException("Fibonacci Retracement lookback period must be at least 10", nameof(Parameters.LookbackPeriod));
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
    
    public void OnNext(IReadOnlyList<HLC<TPrice>> value)
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
            // Fibonacci Retracement outputs 9 levels (7 standard + 2 extensions)
            var outputCount = Parameters.IncludeExtensionLevels ? 9 : 7;
            output = new TOutput[value.Count * outputCount];
        }
        
        OnBarBatch(value, output, 0, 0);
        
        if (s != null && output != null)
        {
            s.OnNext(output);
        }
    }
    
    public void OnNext(HLC<TPrice> value) => OnNext(new[] { value });
    
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
    
    public abstract void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate Fibonacci retracement level based on swing high, swing low, and ratio
    /// Formula: Level = Low + (High - Low) * Ratio
    /// </summary>
    protected TOutput CalculateFibonacciLevel(TOutput swingHigh, TOutput swingLow, double ratio)
    {
        if (swingHigh == MissingOutputValue || swingLow == MissingOutputValue)
            return MissingOutputValue;
            
        var range = swingHigh - swingLow;
        var fibonacciRatio = TOutput.CreateChecked(ratio);
        return swingLow + (range * fibonacciRatio);
    }

    #endregion
}
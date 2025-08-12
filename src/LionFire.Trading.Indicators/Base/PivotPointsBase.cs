using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Pivot Points implementations with common logic
/// </summary>
public abstract class PivotPointsBase<TInput, TOutput>
    : IPivotPoints<TInput, TOutput>
    , IIndicator2<PPivotPoints<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PPivotPoints<TInput, TOutput> Parameters { get; }

    public PivotPointsPeriod PeriodType => Parameters.PeriodType;
    
    public abstract TOutput PivotPoint { get; }
    
    public abstract TOutput Resistance1 { get; }
    
    public abstract TOutput Support1 { get; }
    
    public abstract TOutput Resistance2 { get; }
    
    public abstract TOutput Support2 { get; }
    
    public abstract TOutput Resistance3 { get; }
    
    public abstract TOutput Support3 { get; }

    public int MaxLookback => 1; // Only needs previous period data

    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected PivotPointsBase(PPivotPoints<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
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
            // For Pivot Points, we output 7 values per input: P, R1, S1, R2, S2, R3, S3
            output = new TOutput[value.Count * 7];
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
    
    #region Helper Methods

    /// <summary>
    /// Extracts OHLC data from input
    /// </summary>
    protected virtual (TOutput open, TOutput high, TOutput low, TOutput close) ExtractOHLC(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Look for OHLC properties
        var openProperty = inputType.GetProperty("Open");
        var highProperty = inputType.GetProperty("High");
        var lowProperty = inputType.GetProperty("Low");
        var closeProperty = inputType.GetProperty("Close");
        
        if (openProperty != null && highProperty != null && lowProperty != null && closeProperty != null)
        {
            var openValue = openProperty.GetValue(boxed);
            var highValue = highProperty.GetValue(boxed);
            var lowValue = lowProperty.GetValue(boxed);
            var closeValue = closeProperty.GetValue(boxed);
            
            return (
                TOutput.CreateChecked(Convert.ToDecimal(openValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(highValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(lowValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(closeValue!))
            );
        }
        
        throw new InvalidOperationException(
            $"Unable to extract OHLC data from input type {inputType.Name}. " +
            "Input must have Open, High, Low, and Close properties.");
    }

    /// <summary>
    /// Calculate pivot points from OHLC data
    /// </summary>
    protected static (TOutput pivotPoint, TOutput r1, TOutput s1, TOutput r2, TOutput s2, TOutput r3, TOutput s3) 
        CalculatePivotPoints(TOutput high, TOutput low, TOutput close)
    {
        var three = TOutput.CreateChecked(3);
        var two = TOutput.CreateChecked(2);
        
        // Pivot Point = (High + Low + Close) / 3
        var pivotPoint = (high + low + close) / three;
        
        // Resistance 1 = (2 × P) - Low
        var r1 = (two * pivotPoint) - low;
        
        // Support 1 = (2 × P) - High  
        var s1 = (two * pivotPoint) - high;
        
        // Resistance 2 = P + (High - Low)
        var r2 = pivotPoint + (high - low);
        
        // Support 2 = P - (High - Low)
        var s2 = pivotPoint - (high - low);
        
        // Resistance 3 = High + 2 × (P - Low)
        var r3 = high + two * (pivotPoint - low);
        
        // Support 3 = Low - 2 × (High - P)
        var s3 = low - two * (high - pivotPoint);
        
        return (pivotPoint, r1, s1, r2, s2, r3, s3);
    }

    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
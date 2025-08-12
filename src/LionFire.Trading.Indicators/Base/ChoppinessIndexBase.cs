using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Choppiness Index implementations with common logic
/// </summary>
public abstract class ChoppinessIndexBase<TInput, TOutput>
    : IChoppinessIndex<TInput, TOutput>
    , IIndicator2<PChoppinessIndex<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PChoppinessIndex<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public TOutput ChoppyThreshold => Parameters.ChoppyThreshold;
    
    public TOutput TrendingThreshold => Parameters.TrendingThreshold;
    
    public abstract TOutput CurrentValue { get; }
    
    public virtual bool IsChoppy => IsReady && CurrentValue > ChoppyThreshold;
    
    public virtual bool IsTrending => IsReady && CurrentValue < TrendingThreshold;

    public abstract TOutput TrueRangeSum { get; }

    public abstract TOutput MaxRange { get; }

    public int MaxLookback => Parameters.Period;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected ChoppinessIndexBase(PChoppinessIndex<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Choppiness Index period must be at least 2", nameof(Parameters.Period));
            
        if (Parameters.ChoppyThreshold <= Parameters.TrendingThreshold)
            throw new ArgumentException("Choppy threshold must be greater than trending threshold");
            
        var hundred = TOutput.CreateChecked(100);
        var zero = TOutput.Zero;
        
        if (Parameters.ChoppyThreshold > hundred || Parameters.ChoppyThreshold < zero)
            throw new ArgumentException("Choppy threshold must be between 0 and 100");
            
        if (Parameters.TrendingThreshold > hundred || Parameters.TrendingThreshold < zero)
            throw new ArgumentException("Trending threshold must be between 0 and 100");
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
    
    #region Helper Methods

    /// <summary>
    /// Calculates True Range for a bar given current and previous HLC values
    /// True Range = MAX(High-Low, ABS(High-PrevClose), ABS(Low-PrevClose))
    /// </summary>
    protected static TOutput CalculateTrueRange(HLC<TInput> current, HLC<TInput> previous)
    {
        var currentHigh = TOutput.CreateChecked(Convert.ToDecimal(current.High));
        var currentLow = TOutput.CreateChecked(Convert.ToDecimal(current.Low));
        var previousClose = TOutput.CreateChecked(Convert.ToDecimal(previous.Close));
        
        var range1 = currentHigh - currentLow;
        var range2 = TOutput.Abs(currentHigh - previousClose);
        var range3 = TOutput.Abs(currentLow - previousClose);
        
        return TOutput.Max(range1, TOutput.Max(range2, range3));
    }

    /// <summary>
    /// Calculates True Range for the first bar (no previous close available)
    /// True Range = High - Low
    /// </summary>
    protected static TOutput CalculateFirstTrueRange(HLC<TInput> current)
    {
        var currentHigh = TOutput.CreateChecked(Convert.ToDecimal(current.High));
        var currentLow = TOutput.CreateChecked(Convert.ToDecimal(current.Low));
        
        return currentHigh - currentLow;
    }

    /// <summary>
    /// Extracts High, Low, Close values from HLC input
    /// </summary>
    protected static (TOutput high, TOutput low, TOutput close) ExtractHLC(HLC<TInput> input)
    {
        return (
            TOutput.CreateChecked(Convert.ToDecimal(input.High)),
            TOutput.CreateChecked(Convert.ToDecimal(input.Low)),
            TOutput.CreateChecked(Convert.ToDecimal(input.Close))
        );
    }

    /// <summary>
    /// Calculates the Choppiness Index value
    /// Choppiness = 100 × LOG10(TrueRangeSum / MaxRange) / LOG10(Period)
    /// </summary>
    protected TOutput CalculateChoppinessIndex(TOutput trueRangeSum, TOutput maxRange)
    {
        if (maxRange == TOutput.Zero || trueRangeSum == TOutput.Zero)
        {
            return TOutput.CreateChecked(50); // Default neutral value
        }

        // Convert to double for logarithm calculation
        var trueRangeSumDouble = Convert.ToDouble(trueRangeSum);
        var maxRangeDouble = Convert.ToDouble(maxRange);
        var periodDouble = Convert.ToDouble(Parameters.Period);

        var logTrueRangeRatio = Math.Log10(trueRangeSumDouble / maxRangeDouble);
        var logPeriod = Math.Log10(periodDouble);
        
        var choppiness = 100.0 * (logTrueRangeRatio / logPeriod);
        
        // Ensure the value is within bounds [0, 100]
        choppiness = Math.Max(0.0, Math.Min(100.0, choppiness));
        
        return TOutput.CreateChecked(choppiness);
    }

    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<HLC<TInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
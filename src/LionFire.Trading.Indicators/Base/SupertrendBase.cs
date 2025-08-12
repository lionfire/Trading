using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Supertrend implementations with common logic
/// </summary>
public abstract class SupertrendBase<TInput, TOutput>
    : ISupertrend<TInput, TOutput>
    , IIndicator2<PSupertrend<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PSupertrend<TInput, TOutput> Parameters { get; }

    public int AtrPeriod => Parameters.AtrPeriod;
    
    public TOutput Multiplier => Parameters.Multiplier;
    
    public abstract TOutput Value { get; }
    
    public abstract int TrendDirection { get; }
    
    public virtual bool IsUptrend => TrendDirection > 0;
    
    public virtual bool IsDowntrend => TrendDirection < 0;
    
    public abstract TOutput CurrentATR { get; }

    public int MaxLookback => Parameters.AtrPeriod;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected SupertrendBase(PSupertrend<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.AtrPeriod < 1)
            throw new ArgumentException("ATR period must be at least 1", nameof(Parameters.AtrPeriod));
            
        if (Parameters.Multiplier <= TOutput.Zero)
            throw new ArgumentException("Multiplier must be greater than zero");
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
    
    #region Protected Helpers
    
    /// <summary>
    /// Calculates the true range for a given HLC bar and previous close
    /// </summary>
    protected static TOutput CalculateTrueRange(HLC<TInput> current, TInput? previousClose)
    {
        var currentHigh = TOutput.CreateChecked(Convert.ToDecimal(current.High));
        var currentLow = TOutput.CreateChecked(Convert.ToDecimal(current.Low));
        var currentClose = TOutput.CreateChecked(Convert.ToDecimal(current.Close));
        
        if (previousClose == null)
        {
            // First bar: TR = High - Low
            return currentHigh - currentLow;
        }
        
        var prevClose = TOutput.CreateChecked(Convert.ToDecimal(previousClose.Value));
        
        // TR = max(High - Low, |High - PrevClose|, |Low - PrevClose|)
        var highLow = currentHigh - currentLow;
        var highPrevClose = TOutput.Abs(currentHigh - prevClose);
        var lowPrevClose = TOutput.Abs(currentLow - prevClose);
        
        return TOutput.Max(highLow, TOutput.Max(highPrevClose, lowPrevClose));
    }
    
    /// <summary>
    /// Calculates the basic upper band
    /// </summary>
    protected TOutput CalculateBasicUpperBand(HLC<TInput> hlc, TOutput atr)
    {
        var high = TOutput.CreateChecked(Convert.ToDecimal(hlc.High));
        var low = TOutput.CreateChecked(Convert.ToDecimal(hlc.Low));
        var midpoint = (high + low) / TOutput.CreateChecked(2);
        return midpoint + (Multiplier * atr);
    }
    
    /// <summary>
    /// Calculates the basic lower band
    /// </summary>
    protected TOutput CalculateBasicLowerBand(HLC<TInput> hlc, TOutput atr)
    {
        var high = TOutput.CreateChecked(Convert.ToDecimal(hlc.High));
        var low = TOutput.CreateChecked(Convert.ToDecimal(hlc.Low));
        var midpoint = (high + low) / TOutput.CreateChecked(2);
        return midpoint - (Multiplier * atr);
    }
    
    #endregion
}
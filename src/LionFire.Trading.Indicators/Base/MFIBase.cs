using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for MFI implementations with common logic
/// </summary>
public abstract class MFIBase<TInput, TOutput>
    : IMFI<TInput, TOutput>
    , IIndicator2<PMFI<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PMFI<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public TOutput OverboughtLevel => Parameters.OverboughtLevel;
    
    public TOutput OversoldLevel => Parameters.OversoldLevel;
    
    public abstract TOutput CurrentValue { get; }
    
    public virtual bool IsOverbought => IsReady && CurrentValue > OverboughtLevel;
    
    public virtual bool IsOversold => IsReady && CurrentValue < OversoldLevel;

    public abstract TOutput PositiveMoneyFlow { get; }

    public abstract TOutput NegativeMoneyFlow { get; }

    public virtual TOutput MoneyFlowRatio => 
        NegativeMoneyFlow == TOutput.Zero ? TOutput.CreateChecked(100) : PositiveMoneyFlow / NegativeMoneyFlow;

    public int MaxLookback => Parameters.Period;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected MFIBase(PMFI<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("MFI period must be at least 2", nameof(Parameters.Period));
            
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
    
    #region Helper Methods

    /// <summary>
    /// Extracts OHLCV data from input
    /// </summary>
    protected virtual (TOutput open, TOutput high, TOutput low, TOutput close, TOutput volume) ExtractOHLCV(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Look for OHLCV properties
        var openProperty = inputType.GetProperty("Open");
        var highProperty = inputType.GetProperty("High");
        var lowProperty = inputType.GetProperty("Low");
        var closeProperty = inputType.GetProperty("Close");
        var volumeProperty = inputType.GetProperty("Volume");
        
        if (openProperty != null && highProperty != null && lowProperty != null && 
            closeProperty != null && volumeProperty != null)
        {
            var openValue = openProperty.GetValue(boxed);
            var highValue = highProperty.GetValue(boxed);
            var lowValue = lowProperty.GetValue(boxed);
            var closeValue = closeProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                TOutput.CreateChecked(Convert.ToDecimal(openValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(highValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(lowValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(closeValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(volumeValue!))
            );
        }
        
        throw new InvalidOperationException(
            $"Unable to extract OHLCV data from input type {inputType.Name}. " +
            "Input must have Open, High, Low, Close, and Volume properties.");
    }

    /// <summary>
    /// Calculates typical price (H+L+C)/3
    /// </summary>
    protected static TOutput CalculateTypicalPrice(TOutput high, TOutput low, TOutput close)
    {
        var three = TOutput.CreateChecked(3);
        return (high + low + close) / three;
    }

    /// <summary>
    /// Calculates raw money flow (typical price * volume)
    /// </summary>
    protected static TOutput CalculateRawMoneyFlow(TOutput typicalPrice, TOutput volume)
    {
        return typicalPrice * volume;
    }

    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
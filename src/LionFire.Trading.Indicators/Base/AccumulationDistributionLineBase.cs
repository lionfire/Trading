using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Accumulation/Distribution Line implementations with common logic
/// </summary>
public abstract class AccumulationDistributionLineBase<TInput, TOutput>
    : IAccumulationDistributionLine<TInput, TOutput>
    , IIndicator2<PAccumulationDistributionLine<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PAccumulationDistributionLine<TInput, TOutput> Parameters { get; }

    public abstract TOutput CurrentValue { get; }
    
    public abstract TOutput LastMoneyFlowVolume { get; }
    
    public abstract TOutput LastMoneyFlowMultiplier { get; }
    
    public virtual bool IsAccumulating => LastMoneyFlowVolume > TOutput.Zero;
    
    public virtual bool IsDistributing => LastMoneyFlowVolume < TOutput.Zero;

    public int MaxLookback => 1; // A/D Line only needs current data point
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected AccumulationDistributionLineBase(PAccumulationDistributionLine<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        // A/D Line has minimal parameter requirements
        // Main validation is on the input data having HLCV components
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
    /// Extracts HLCV data from input
    /// </summary>
    protected virtual (TOutput high, TOutput low, TOutput close, TOutput volume) ExtractHLCV(TInput input)
    {
        // Handle different input types
        var inputType = typeof(TInput);
        var boxed = (object)input;
        
        // Look for HLCV properties
        var highProperty = inputType.GetProperty("High");
        var lowProperty = inputType.GetProperty("Low");
        var closeProperty = inputType.GetProperty("Close");
        var volumeProperty = inputType.GetProperty("Volume");
        
        if (highProperty != null && lowProperty != null && 
            closeProperty != null && volumeProperty != null)
        {
            var highValue = highProperty.GetValue(boxed);
            var lowValue = lowProperty.GetValue(boxed);
            var closeValue = closeProperty.GetValue(boxed);
            var volumeValue = volumeProperty.GetValue(boxed);
            
            return (
                TOutput.CreateChecked(Convert.ToDecimal(highValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(lowValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(closeValue!)),
                TOutput.CreateChecked(Convert.ToDecimal(volumeValue!))
            );
        }
        
        throw new InvalidOperationException(
            $"Unable to extract HLCV data from input type {inputType.Name}. " +
            "Input must have High, Low, Close, and Volume properties.");
    }

    /// <summary>
    /// Calculates Money Flow Multiplier = ((Close - Low) - (High - Close)) / (High - Low)
    /// </summary>
    protected static TOutput CalculateMoneyFlowMultiplier(TOutput high, TOutput low, TOutput close)
    {
        var range = high - low;
        
        // Avoid division by zero - when high equals low, there's no price range
        if (range == TOutput.Zero)
            return TOutput.Zero;
            
        return ((close - low) - (high - close)) / range;
    }

    /// <summary>
    /// Calculates Money Flow Volume = Money Flow Multiplier × Volume
    /// </summary>
    protected static TOutput CalculateMoneyFlowVolume(TOutput moneyFlowMultiplier, TOutput volume)
    {
        return moneyFlowMultiplier * volume;
    }

    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
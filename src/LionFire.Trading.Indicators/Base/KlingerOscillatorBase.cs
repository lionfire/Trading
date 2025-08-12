using LionFire.Trading;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Klinger Oscillator implementations with common logic
/// </summary>
public abstract class KlingerOscillatorBase<TInput, TOutput>
    : IKlingerOscillator<TInput, TOutput>
    , IIndicator2<PKlingerOscillator<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PKlingerOscillator<TInput, TOutput> Parameters { get; }

    public int FastPeriod => Parameters.FastPeriod;
    
    public int SlowPeriod => Parameters.SlowPeriod;
    
    public int SignalPeriod => Parameters.SignalPeriod;
    
    public abstract TOutput Klinger { get; }
    
    public abstract TOutput Signal { get; }
    
    public abstract TOutput VolumeForce { get; }
    
    public virtual bool IsBullish => IsReady && Klinger > Signal;
    
    public virtual bool IsBearish => IsReady && Klinger < Signal;

    public int MaxLookback => Parameters.SlowPeriod + Parameters.SignalPeriod - 1;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected KlingerOscillatorBase(PKlingerOscillator<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        Parameters.Validate();
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
    /// Calculates the trend direction based on HLC comparison
    /// </summary>
    protected static TOutput CalculateTrend(TOutput high, TOutput low, TOutput close, 
        TOutput prevHigh, TOutput prevLow, TOutput prevClose)
    {
        var currentTypicalPrice = (high + low + close) / TOutput.CreateChecked(3);
        var previousTypicalPrice = (prevHigh + prevLow + prevClose) / TOutput.CreateChecked(3);
        
        return currentTypicalPrice >= previousTypicalPrice ? TOutput.One : TOutput.CreateChecked(-1);
    }

    /// <summary>
    /// Calculates Daily Movement (DM) = High - Low
    /// </summary>
    protected static TOutput CalculateDailyMovement(TOutput high, TOutput low)
    {
        return high - low;
    }

    /// <summary>
    /// Calculates Volume Force according to Klinger formula
    /// </summary>
    protected static TOutput CalculateVolumeForce(TOutput volume, TOutput trend, 
        TOutput dailyMovement, TOutput cumulativeMovement)
    {
        if (cumulativeMovement == TOutput.Zero)
            return TOutput.Zero;
            
        // Volume Force = Volume × Trend × (2 × (DM/CM - 1)) × 100
        var ratio = dailyMovement / cumulativeMovement;
        var multiplier = TOutput.CreateChecked(2) * (ratio - TOutput.One);
        var volumeForce = volume * trend * multiplier * TOutput.CreateChecked(100);
        
        return volumeForce;
    }

    #endregion
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Klinger Oscillator indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "Klinger",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Klinger Oscillator indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PKlingerOscillator<TInput, TOutput> p)
        => [
            new() {
                Name = "Klinger",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion
}
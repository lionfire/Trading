using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Bollinger Bands implementations with common logic
/// </summary>
public abstract class BollingerBandsBase<TInput, TOutput>
    : IBollingerBands<TInput, TOutput>
    , IIndicator2<PBollingerBands<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PBollingerBands<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public TOutput StandardDeviations => Parameters.StandardDeviations;
    
    public abstract TOutput UpperBand { get; }
    
    public abstract TOutput MiddleBand { get; }
    
    public abstract TOutput LowerBand { get; }
    
    public virtual TOutput BandWidth => IsReady ? UpperBand - LowerBand : TradingValueUtils<TOutput>.MissingValue;
    
    public virtual TOutput PercentB 
    {
        get
        {
            if (!IsReady) return TradingValueUtils<TOutput>.MissingValue;
            
            var bandwidth = BandWidth;
            if (bandwidth == TOutput.Zero) return TOutput.CreateChecked(0.5); // Price at middle when bands are squeezed
            
            // Get the current price (this would typically come from the last input)
            var price = LastPrice;
            if (price == TradingValueUtils<TOutput>.MissingValue) return TradingValueUtils<TOutput>.MissingValue;
            
            return (price - LowerBand) / bandwidth;
        }
    }

    public int MaxLookback => Parameters.Period;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;
    
    protected TOutput LastPrice { get; set; } = TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected BollingerBandsBase(PBollingerBands<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Bollinger Bands period must be at least 2", nameof(Parameters.Period));
            
        var zero = TOutput.Zero;
        if (Parameters.StandardDeviations <= zero)
            throw new ArgumentException("Standard deviations must be greater than 0", nameof(Parameters.StandardDeviations));
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
            // For Bollinger Bands, we output 3 values per input: Upper, Middle, Lower
            output = new TOutput[value.Count * 3];
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
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Keltner Channels implementations with common logic
/// </summary>
public abstract class KeltnerChannelsBase<TInput, TOutput>
    : IKeltnerChannels<TInput, TOutput>
    , IIndicator2<PKeltnerChannels<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PKeltnerChannels<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public int AtrPeriod => Parameters.AtrPeriod;
    
    public TOutput AtrMultiplier => Parameters.AtrMultiplier;
    
    public abstract TOutput UpperBand { get; }
    
    public abstract TOutput MiddleLine { get; }
    
    public abstract TOutput LowerBand { get; }
    
    public abstract TOutput AtrValue { get; }
    
    public virtual TOutput ChannelWidth => IsReady ? UpperBand - LowerBand : TradingValueUtils<TOutput>.MissingValue;

    public int MaxLookback => Math.Max(Parameters.Period, Parameters.AtrPeriod);
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected KeltnerChannelsBase(PKeltnerChannels<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Keltner Channels EMA period must be at least 2", nameof(Parameters.Period));
            
        if (Parameters.AtrPeriod < 2)
            throw new ArgumentException("ATR period must be at least 2", nameof(Parameters.AtrPeriod));
            
        var zero = TOutput.Zero;
        if (Parameters.AtrMultiplier <= zero)
            throw new ArgumentException("ATR multiplier must be greater than 0", nameof(Parameters.AtrMultiplier));
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
            // For Keltner Channels, we output 3 values per input: Upper, Middle, Lower
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
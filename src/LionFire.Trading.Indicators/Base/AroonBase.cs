using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Aroon implementations with common logic
/// </summary>
public abstract class AroonBase<TInput, TOutput>
    : IAroon<TInput, TOutput>
    , IIndicator2<PAroon<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PAroon<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;
    
    public TOutput UptrendThreshold => Parameters.UptrendThreshold;
    
    public TOutput DowntrendThreshold => Parameters.DowntrendThreshold;
    
    public abstract TOutput AroonUp { get; }
    
    public abstract TOutput AroonDown { get; }
    
    public virtual TOutput AroonOscillator => AroonUp - AroonDown;
    
    public virtual bool IsUptrend 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(100) - UptrendThreshold;
            return AroonUp > UptrendThreshold && AroonDown < lowThreshold;
        }
    }
    
    public virtual bool IsDowntrend 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(100) - DowntrendThreshold;
            return AroonDown > DowntrendThreshold && AroonUp < lowThreshold;
        }
    }
    
    public virtual bool IsConsolidating 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(100) - TOutput.CreateChecked(70);
            var highThreshold = TOutput.CreateChecked(70);
            return AroonUp >= lowThreshold && AroonUp <= highThreshold 
                && AroonDown >= lowThreshold && AroonDown <= highThreshold;
        }
    }

    public int MaxLookback => Parameters.Period;
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected AroonBase(PAroon<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Aroon period must be at least 2", nameof(Parameters.Period));
            
        var hundred = TOutput.CreateChecked(100);
        var zero = TOutput.Zero;
        
        if (Parameters.UptrendThreshold > hundred || Parameters.UptrendThreshold < zero)
            throw new ArgumentException("Uptrend threshold must be between 0 and 100");
            
        if (Parameters.DowntrendThreshold > hundred || Parameters.DowntrendThreshold < zero)
            throw new ArgumentException("Downtrend threshold must be between 0 and 100");
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
            output = new TOutput[value.Count * 3]; // AroonUp, AroonDown, AroonOscillator
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
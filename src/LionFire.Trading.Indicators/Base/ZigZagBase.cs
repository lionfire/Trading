using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for ZigZag implementations with common logic
/// </summary>
public abstract class ZigZagBase<TInput, TOutput>
    : IZigZag<TInput, TOutput>
    , IIndicator2<PZigZag<TInput, TOutput>, TInput, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<TInput>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PZigZag<TInput, TOutput> Parameters { get; }

    public TOutput Deviation => Parameters.Deviation;
    
    public int Depth => Parameters.Depth;
    
    public abstract TOutput CurrentValue { get; }
    
    public abstract TOutput LastPivotHigh { get; }
    
    public abstract TOutput LastPivotLow { get; }
    
    public abstract int Direction { get; }

    public int MaxLookback => Parameters.Depth;
    
    public abstract bool IsReady { get; }
    
    public abstract IReadOnlyList<ZigZagPivot<TOutput>>? RecentPivots { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected ZigZagBase(PZigZag<TInput, TOutput> parameters)
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
    
    #region Abstract Methods
    
    public abstract void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculates the percentage change between two values
    /// </summary>
    protected static TOutput CalculatePercentageChange(TOutput from, TOutput to)
    {
        if (from == TOutput.Zero)
            return TOutput.Zero;
            
        var change = to - from;
        var percentChange = (change / from) * TOutput.CreateChecked(100);
        return percentChange;
    }

    /// <summary>
    /// Checks if the price change meets the minimum deviation threshold
    /// </summary>
    protected bool MeetsDeviationThreshold(TOutput from, TOutput to)
    {
        var percentChange = CalculatePercentageChange(from, to);
        var absChange = percentChange < TOutput.Zero ? -percentChange : percentChange;
        return absChange >= Deviation;
    }

    #endregion
}
using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Parabolic SAR implementations with common logic
/// </summary>
public abstract class ParabolicSARBase<TPrice, TOutput>
    : IParabolicSAR<HLC<TPrice>, TOutput>
    , IIndicator2<PParabolicSAR<TPrice, TOutput>, HLC<TPrice>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TPrice>>>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PParabolicSAR<TPrice, TOutput> Parameters { get; }

    public TOutput AccelerationFactor => Parameters.AccelerationFactor;
    
    public TOutput MaxAccelerationFactor => Parameters.MaxAccelerationFactor;
    
    public abstract TOutput CurrentValue { get; }
    
    public abstract bool IsLong { get; }
    
    public abstract bool HasReversed { get; }
    
    public abstract TOutput CurrentAccelerationFactor { get; }

    public int MaxLookback => Parameters.LookbackForInputSlot(PParabolicSAR<TPrice, TOutput>.GetInputSlots().First());
    
    public abstract bool IsReady { get; }
    
    protected Subject<IReadOnlyList<TOutput>>? subject;
    
    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected ParabolicSARBase(PParabolicSAR<TPrice, TOutput> parameters)
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
    
    public void OnNext(IReadOnlyList<HLC<TPrice>> value)
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
    
    public void OnNext(HLC<TPrice> value) => OnNext(new[] { value });
    
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
    
    public abstract void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0);
    
    public abstract void Clear();
    
    #endregion
}
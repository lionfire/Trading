using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;
using LionFire.Trading;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for ADX (Average Directional Index) implementations with common logic
/// </summary>
public abstract class ADXBase<TInput, TOutput>
    : IADX<TInput, TOutput>
    , IIndicator2<PADX<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PADX<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;

    public TOutput StrongTrendThreshold => Parameters.StrongTrendThreshold;

    public TOutput VeryStrongTrendThreshold => Parameters.VeryStrongTrendThreshold;

    /// <summary>
    /// Current ADX value (0-100) - measures trend strength
    /// </summary>
    public abstract TOutput ADX { get; }

    /// <summary>
    /// Current Plus Directional Indicator (+DI) value (0-100)
    /// </summary>
    public abstract TOutput PlusDI { get; }

    /// <summary>
    /// Current Minus Directional Indicator (-DI) value (0-100)
    /// </summary>
    public abstract TOutput MinusDI { get; }

    /// <summary>
    /// Indicates whether ADX shows a strong trend (above strong trend threshold)
    /// </summary>
    public virtual bool IsStrongTrend => IsReady && ADX > StrongTrendThreshold;

    /// <summary>
    /// Indicates whether ADX shows a very strong trend (above very strong trend threshold)
    /// </summary>
    public virtual bool IsVeryStrongTrend => IsReady && ADX > VeryStrongTrendThreshold;

    /// <summary>
    /// Indicates bullish directional movement (+DI > -DI)
    /// </summary>
    public virtual bool IsBullish => IsReady && PlusDI > MinusDI;

    /// <summary>
    /// Indicates bearish directional movement (-DI > +DI)
    /// </summary>
    public virtual bool IsBearish => IsReady && MinusDI > PlusDI;

    public int MaxLookback => Parameters.Period * 2; // Need extra period for initial smoothing

    public abstract bool IsReady { get; }

    protected Subject<IReadOnlyList<TOutput>>? subject;

    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected ADXBase(PADX<TInput, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        if (Parameters.Period < 2)
            throw new ArgumentException("Period must be at least 2", nameof(Parameters.Period));

        var hundred = TOutput.CreateChecked(100);
        var zero = TOutput.Zero;

        if (Parameters.StrongTrendThreshold > hundred || Parameters.StrongTrendThreshold < zero)
            throw new ArgumentException("Strong trend threshold must be between 0 and 100");

        if (Parameters.VeryStrongTrendThreshold > hundred || Parameters.VeryStrongTrendThreshold < zero)
            throw new ArgumentException("Very strong trend threshold must be between 0 and 100");

        if (Parameters.VeryStrongTrendThreshold <= Parameters.StrongTrendThreshold)
            throw new ArgumentException("Very strong trend threshold must be greater than strong trend threshold");
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
            // ADX has three outputs: ADX, +DI, -DI
            output = new TOutput[value.Count * 3];
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

    #region Static

    /// <summary>
    /// Gets the output slots for the ADX indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "ADX",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "PlusDI",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MinusDI",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the ADX indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PADX<TInput, TOutput> p)
        => [
            new() {
                Name = "ADX",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "PlusDI",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "MinusDI",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion
}
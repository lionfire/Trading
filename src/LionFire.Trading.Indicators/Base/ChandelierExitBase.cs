using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Chandelier Exit implementations with common logic
/// </summary>
public abstract class ChandelierExitBase<TInput, TOutput>
    : IChandelierExit<TInput, TOutput>
    , IIndicator2<PChandelierExit<TInput, TOutput>, HLC<TInput>, TOutput>
    , IObservable<IReadOnlyList<TOutput>>
    , IObserver<IReadOnlyList<HLC<TInput>>>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Properties

    protected PChandelierExit<TInput, TOutput> Parameters { get; }

    public int Period => Parameters.Period;

    public TOutput AtrMultiplier => Parameters.AtrMultiplier;

    public abstract TOutput ExitLong { get; }

    public abstract TOutput ExitShort { get; }

    public abstract TOutput CurrentATR { get; }

    public abstract TOutput HighestHigh { get; }

    public abstract TOutput LowestLow { get; }

    public int MaxLookback => Parameters.Period;

    public abstract bool IsReady { get; }

    protected Subject<IReadOnlyList<TOutput>>? subject;

    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Lifecycle

    protected ChandelierExitBase(PChandelierExit<TInput, TOutput> parameters)
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

        if (Parameters.AtrMultiplier <= TOutput.Zero)
            throw new ArgumentException("AtrMultiplier must be greater than zero");
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

    #endregion
}

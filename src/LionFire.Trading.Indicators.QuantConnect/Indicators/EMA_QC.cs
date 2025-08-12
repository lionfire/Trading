using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Exponential Moving Average (EMA) indicator.
/// EMA gives more weight to recent prices using exponential smoothing.
/// </summary>
public class EMA_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<EMA_QC<TPrice, TOutput>, global::QuantConnect.Indicators.ExponentialMovingAverage, PEMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<EMA_QC<TPrice, TOutput>, PEMA<TPrice, TOutput>, TPrice, TOutput>,
    IEMA<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the EMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the EMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PEMA<TPrice, TOutput> p)
        => [new() {
                Name = "EMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The EMA parameters
    /// </summary>
    public readonly PEMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for EMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    /// <summary>
    /// The smoothing factor used in EMA calculation
    /// </summary>
    public TOutput SmoothingFactor { get; private set; }

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new EMA indicator instance
    /// </summary>
    public static EMA_QC<TPrice, TOutput> Create(PEMA<TPrice, TOutput> p) => new EMA_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the EMA indicator
    /// </summary>
    public EMA_QC(PEMA<TPrice, TOutput> parameters) : base(
        CreateEmaIndicator(parameters))
    {
        Parameters = parameters;
        SmoothingFactor = parameters.GetEffectiveSmoothingFactor();
    }

    private static global::QuantConnect.Indicators.ExponentialMovingAverage CreateEmaIndicator(PEMA<TPrice, TOutput> parameters)
    {
        // If smoothing factor is specified, convert it to decimal for QuantConnect
        if (parameters.SmoothingFactor.HasValue && !parameters.SmoothingFactor.Value.Equals(TOutput.Zero))
        {
            var smoothingFactorDecimal = Convert.ToDecimal(parameters.SmoothingFactor.Value);
            return new global::QuantConnect.Indicators.ExponentialMovingAverage(
                parameters.Period, 
                smoothingFactorDecimal);
        }
        else
        {
            // Let QuantConnect calculate the default smoothing factor
            return new global::QuantConnect.Indicators.ExponentialMovingAverage(parameters.Period);
        }
    }

    #endregion

    #region State

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the current EMA value
    /// </summary>
    public TOutput Value => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create an IndicatorDataPoint with the price value
            var dataPoint = new global::QuantConnect.Indicators.IndicatorDataPoint(
                time: endTime,
                value: Convert.ToDecimal(input));

            WrappedIndicator.Update(dataPoint);

            endTime += period;

            // Output the EMA value if ready
            if (WrappedIndicator.IsReady)
            {
                var emaValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { emaValue });
                }
                
                OnNext_PopulateOutput(emaValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Helper method to populate the output buffer
    /// </summary>
    private static void OnNext_PopulateOutput(TOutput value, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) 
        { 
            outputSkip--; 
        }
        else if (outputBuffer != null) 
        {
            outputBuffer[outputIndex++] = value;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear(); 
        WrappedIndicator.Reset();
        endTime = DefaultEndTime;
    }

    #endregion
}
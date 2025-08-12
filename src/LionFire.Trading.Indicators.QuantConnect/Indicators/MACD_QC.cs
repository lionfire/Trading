using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of MACD (Moving Average Convergence Divergence) indicator.
/// MACD is a trend-following momentum indicator that shows the relationship between two moving averages.
/// </summary>
public class MACD_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<MACD_QC<TPrice, TOutput>, global::QuantConnect.Indicators.MovingAverageConvergenceDivergence, PMACD<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<MACD_QC<TPrice, TOutput>, PMACD<TPrice, TOutput>, TPrice, TOutput>,
    IMACD<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the MACD indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the MACD indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PMACD<TPrice, TOutput> p)
        => [
            new() {
                Name = "MACD",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Histogram",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The MACD parameters
    /// </summary>
    public readonly PMACD<TPrice, TOutput> Parameters;

    /// <summary>
    /// The fast period for EMA calculation
    /// </summary>
    public int FastPeriod => Parameters.FastPeriod;

    /// <summary>
    /// The slow period for EMA calculation
    /// </summary>
    public int SlowPeriod => Parameters.SlowPeriod;

    /// <summary>
    /// The signal period for EMA calculation of the MACD line
    /// </summary>
    public int SignalPeriod => Parameters.SignalPeriod;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.SlowPeriod + Parameters.SignalPeriod - 1;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new MACD indicator instance
    /// </summary>
    public static MACD_QC<TPrice, TOutput> Create(PMACD<TPrice, TOutput> p) => new MACD_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the MACD indicator
    /// </summary>
    public MACD_QC(PMACD<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.MovingAverageConvergenceDivergence(parameters.FastPeriod, parameters.SlowPeriod, parameters.SignalPeriod))
    {
        Parameters = parameters;
        Parameters.Validate();
    }

    #endregion

    #region State

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the current MACD line value (Fast EMA - Slow EMA)
    /// </summary>
    public TOutput MACD => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Signal line value (EMA of MACD line)
    /// </summary>
    public TOutput Signal => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Signal.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Histogram value (MACD - Signal)
    /// </summary>
    public TOutput Histogram => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Histogram.Current.Price) : default(TOutput)!;

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

            // Output the MACD values if ready
            if (WrappedIndicator.IsReady)
            {
                var macdValue = ConvertToOutput(WrappedIndicator.Current.Price);
                var signalValue = ConvertToOutput(WrappedIndicator.Signal.Current.Price);
                var histogramValue = ConvertToOutput(WrappedIndicator.Histogram.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { macdValue, signalValue, histogramValue });
                }
                
                OnNext_PopulateOutput(macdValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
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
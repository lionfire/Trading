using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Simple Moving Average (SMA) indicator.
/// SMA calculates the arithmetic mean of a given number of prices over a specific period.
/// </summary>
public class SMA_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<SMA_QC<TPrice, TOutput>, global::QuantConnect.Indicators.SimpleMovingAverage, PSMA<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<SMA_QC<TPrice, TOutput>, PSMA<TPrice, TOutput>, TPrice, TOutput>,
    ISMA<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the SMA indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the SMA indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PSMA<TPrice, TOutput> p)
        => [new() {
                Name = "SMA",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The SMA parameters
    /// </summary>
    public readonly PSMA<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for SMA calculation
    /// </summary>
    public int Period => Parameters.Period;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new SMA indicator instance
    /// </summary>
    public static SMA_QC<TPrice, TOutput> Create(PSMA<TPrice, TOutput> p) => new SMA_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the SMA indicator
    /// </summary>
    public SMA_QC(PSMA<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.SimpleMovingAverage(parameters.Period))
    {
        Parameters = parameters;
    }

    #endregion

    #region State

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the current SMA value
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

            // Output the SMA value if ready
            if (WrappedIndicator.IsReady)
            {
                var smaValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { smaValue });
                }
                
                OnNext_PopulateOutput(smaValue, output, ref outputIndex, ref outputSkip);
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
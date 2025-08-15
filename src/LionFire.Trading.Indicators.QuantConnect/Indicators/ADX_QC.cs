using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of ADX (Average Directional Index) indicator.
/// ADX measures the strength of a trend regardless of direction.
/// </summary>
public class ADX_QC<TInput, TOutput> : QuantConnectIndicatorWrapper<ADX_QC<TInput, TOutput>, global::QuantConnect.Indicators.AverageDirectionalIndex, PADX<TInput, TOutput>, HLC<TInput>, TOutput>,
    IIndicator2<ADX_QC<TInput, TOutput>, PADX<TInput, TOutput>, HLC<TInput>, TOutput>,
    IADX<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
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

    #region Parameters

    /// <summary>
    /// The ADX parameters
    /// </summary>
    public readonly PADX<TInput, TOutput> Parameters;

    /// <summary>
    /// The period used for ADX calculation
    /// </summary>
    public int Period => Parameters.Period;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period * 2;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new ADX indicator instance
    /// </summary>
    public static ADX_QC<TInput, TOutput> Create(PADX<TInput, TOutput> p) => new ADX_QC<TInput, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the ADX indicator
    /// </summary>
    public ADX_QC(PADX<TInput, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.AverageDirectionalIndex(parameters.Period))
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
    /// Gets the current ADX value (0-100) - measures trend strength
    /// </summary>
    public TOutput ADX => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Plus Directional Indicator (+DI) value (0-100)
    /// </summary>
    public TOutput PlusDI => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.PositiveDirectionalIndex.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Minus Directional Indicator (-DI) value (0-100)
    /// </summary>
    public TOutput MinusDI => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.NegativeDirectionalIndex.Current.Price) : default(TOutput)!;

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create a TradeBar with HLC data
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input.Close), // Use close as open since we don't have open
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the ADX values if ready
            if (WrappedIndicator.IsReady)
            {
                var adxValue = ConvertToOutput(WrappedIndicator.Current.Price);
                var plusDIValue = ConvertToOutput(WrappedIndicator.PositiveDirectionalIndex.Current.Price);
                var minusDIValue = ConvertToOutput(WrappedIndicator.NegativeDirectionalIndex.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { adxValue, plusDIValue, minusDIValue });
                }
                
                OnNext_PopulateOutput(adxValue, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(plusDIValue, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(minusDIValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
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
        else if (outputBuffer != null && outputIndex < outputBuffer.Length) 
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
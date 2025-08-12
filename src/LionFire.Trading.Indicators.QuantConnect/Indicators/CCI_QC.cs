using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of CCI (Commodity Channel Index) indicator.
/// CCI calculates the deviation of typical price from its simple moving average, normalized by mean deviation.
/// Formula: CCI = (Typical Price - SMA of Typical Price) / (0.015 * Mean Deviation)
/// where Typical Price = (High + Low + Close) / 3
/// </summary>
public class CCI_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<CCI_QC<TPrice, TOutput>, global::QuantConnect.Indicators.CommodityChannelIndex, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIndicator2<CCI_QC<TPrice, TOutput>, PCCI<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    ICCI<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the CCI indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    /// <summary>
    /// Gets the output slots for the CCI indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PCCI<TPrice, TOutput> p)
        => [new() {
                Name = "CCI",
                ValueType = typeof(TOutput),
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The CCI parameters
    /// </summary>
    public readonly PCCI<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for CCI calculation
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
    /// Creates a new CCI indicator instance
    /// </summary>
    public static CCI_QC<TPrice, TOutput> Create(PCCI<TPrice, TOutput> p) => new CCI_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the CCI indicator
    /// </summary>
    public CCI_QC(PCCI<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.CommodityChannelIndex(parameters.Period))
    {
        Parameters = parameters;
        
        // Note: QuantConnect's CCI uses a fixed constant of 0.015, but our parameters allow customization
        // If the parameter constant differs from 0.015, we'll need to scale the result accordingly
        constantScaleFactor = 0.015 / parameters.Constant;
    }

    #endregion

    #region State

    private readonly double constantScaleFactor;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => WrappedIndicator.IsReady;

    /// <summary>
    /// Gets the current CCI value
    /// </summary>
    public TOutput Value => WrappedIndicator.IsReady ? 
        ConvertToOutput(WrappedIndicator.Current.Price * (decimal)constantScaleFactor) : 
        default(TOutput)!;

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
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create a TradeBar with the HLC data for QuantConnect's CCI
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.Empty, // Using empty symbol for indicator calculations
                open: Convert.ToDecimal(input.Close), // Use close as open since we don't have open
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
                volume: 0 // Volume not needed for CCI calculation
            );

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the CCI value if ready
            if (WrappedIndicator.IsReady)
            {
                var cciValue = ConvertToOutput(WrappedIndicator.Current.Price * (decimal)constantScaleFactor);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { cciValue });
                }
                
                OnNext_PopulateOutput(cciValue, output, ref outputIndex, ref outputSkip);
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
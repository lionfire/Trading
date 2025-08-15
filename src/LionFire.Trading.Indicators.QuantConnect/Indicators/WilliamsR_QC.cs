using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Williams %R indicator.
/// Williams %R is a momentum oscillator that measures overbought and oversold levels.
/// It compares the closing price to the high-low range over a specific period.
/// Values range from -100 to 0, with readings from -80 to -100 typically considered 
/// oversold and readings from -20 to 0 considered overbought.
/// </summary>
public class WilliamsR_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<WilliamsR_QC<TPrice, TOutput>, global::QuantConnect.Indicators.WilliamsPercentR, PWilliamsR<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IIndicator2<WilliamsR_QC<TPrice, TOutput>, PWilliamsR<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Williams %R indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "WilliamsR",
                ValueType = typeof(TOutput),
                // Description = "Williams %R value (-100 to 0)"
            }];

    /// <summary>
    /// Gets the output slots for the Williams %R indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PWilliamsR<TPrice, TOutput> p)
        => [new() {
                Name = "WilliamsR",
                ValueType = typeof(TOutput),
                // Description = "Williams %R value (-100 to 0)"
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The Williams %R parameters
    /// </summary>
    public readonly PWilliamsR<TPrice, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.Period;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Williams %R indicator instance
    /// </summary>
    public static WilliamsR_QC<TPrice, TOutput> Create(PWilliamsR<TPrice, TOutput> p) => new WilliamsR_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Williams %R indicator
    /// </summary>
    public WilliamsR_QC(PWilliamsR<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.WilliamsPercentR(parameters.Period))
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
    /// Gets the current Williams %R value
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Checks if the Williams %R indicates overbought conditions
    /// </summary>
    public bool IsOverbought => WrappedIndicator.IsReady && 
        ConvertToOutput(WrappedIndicator.Current.Price) > Parameters.OverboughtLevel;

    /// <summary>
    /// Checks if the Williams %R indicates oversold conditions
    /// </summary>
    public bool IsOversold => WrappedIndicator.IsReady && 
        ConvertToOutput(WrappedIndicator.Current.Price) < Parameters.OversoldLevel;

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
            // Create a TradeBar with the HLC values
            // Williams %R requires High, Low, and Close prices
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: global::QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input.Close), // Open not used by Williams %R, use Close
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the Williams %R value if ready
            if (WrappedIndicator.IsReady)
            {
                var williamsRValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { williamsRValue });
                }
                
                OnNext_PopulateOutput(williamsRValue, output, ref outputIndex, ref outputSkip);
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
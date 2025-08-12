using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Parabolic SAR (Stop and Reverse) indicator.
/// The Parabolic SAR is a trend-following indicator that provides potential reversal points
/// using an acceleration factor that increases as the trend develops.
/// </summary>
public class ParabolicSAR_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<ParabolicSAR_QC<TPrice, TOutput>, global::QuantConnect.Indicators.ParabolicStopAndReverse, PParabolicSAR<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IIndicator2<ParabolicSAR_QC<TPrice, TOutput>, PParabolicSAR<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IParabolicSAR<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Parabolic SAR indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [new() {
                Name = "SAR",
                ValueType = typeof(TOutput),
                // Description = "Parabolic Stop and Reverse value"
            }];

    /// <summary>
    /// Gets the output slots for the Parabolic SAR indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PParabolicSAR<TPrice, TOutput> p)
        => [new() {
                Name = "SAR",
                ValueType = typeof(TOutput),
                // Description = "Parabolic Stop and Reverse value"
            }];

    #endregion

    #region Parameters

    /// <summary>
    /// The Parabolic SAR parameters
    /// </summary>
    public readonly PParabolicSAR<TPrice, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.LookbackForInputSlot(Parameters.GetInputSlots().First());

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Parabolic SAR indicator instance
    /// </summary>
    public static ParabolicSAR_QC<TPrice, TOutput> Create(PParabolicSAR<TPrice, TOutput> p) => new ParabolicSAR_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Parabolic SAR indicator
    /// </summary>
    public ParabolicSAR_QC(PParabolicSAR<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.ParabolicStopAndReverse(
            Convert.ToDecimal(parameters.AccelerationFactor), 
            Convert.ToDecimal(parameters.AccelerationFactor),
            Convert.ToDecimal(parameters.MaxAccelerationFactor)))
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
    /// Gets the current SAR value
    /// </summary>
    public TOutput CurrentValue => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the acceleration factor parameter
    /// </summary>
    public TOutput AccelerationFactor => Parameters.AccelerationFactor;
    
    /// <summary>
    /// Gets the maximum acceleration factor parameter
    /// </summary>
    public TOutput MaxAccelerationFactor => Parameters.MaxAccelerationFactor;
    
    /// <summary>
    /// Gets the current trend direction (true for long/uptrend, false for short/downtrend)
    /// Note: QuantConnect's PSAR doesn't directly expose trend direction,
    /// so we determine it by comparing current price to SAR
    /// </summary>
    public bool IsLong
    {
        get
        {
            if (!WrappedIndicator.IsReady || lastHLC == null)
                return true; // Default assumption
                
            var currentPrice = TOutput.CreateChecked(Convert.ToDecimal(lastHLC.Value.High + lastHLC.Value.Low) / 2.0m);
            return currentPrice > CurrentValue;
        }
    }
    
    /// <summary>
    /// Indicates if the SAR has recently switched direction
    /// Note: This is approximated by checking if the trend changed from the previous calculation
    /// </summary>
    public bool HasReversed { get; private set; }
    
    /// <summary>
    /// Gets the current acceleration factor being used in calculations
    /// Note: QuantConnect's PSAR doesn't expose the current AF, so we return the base AF
    /// </summary>
    public TOutput CurrentAccelerationFactor => Parameters.AccelerationFactor;

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;
    private bool previousIsLong = true;
    private HLC<TPrice>? lastHLC;

    #endregion

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            lastHLC = input;
            
            // Create a TradeBar with the HLC values
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input.High), // Use high as open for simplicity
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Low), // Use low as close for simplicity
                volume: 0,
                period: period);

            // Check for reversal before updating
            bool currentIsLong = IsLong;
            
            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Check if trend reversed after the update
            bool newIsLong = IsLong;
            HasReversed = (currentIsLong != newIsLong);
            previousIsLong = newIsLong;

            // Output the SAR value if ready
            if (WrappedIndicator.IsReady)
            {
                var sarValue = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { sarValue });
                }
                
                OnNext_PopulateOutput(sarValue, output, ref outputIndex, ref outputSkip);
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
        HasReversed = false;
        previousIsLong = true;
        lastHLC = null;
    }

    #endregion
}
using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using QuantConnect.Data.Market;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Aroon indicator.
/// Wraps QuantConnect's AroonOscillator to provide Aroon Up, Aroon Down, and Aroon Oscillator values.
/// </summary>
public class Aroon_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<Aroon_QC<TPrice, TOutput>, global::QuantConnect.Indicators.AroonOscillator, PAroon<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIndicator2<Aroon_QC<TPrice, TOutput>, PAroon<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IAroon<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Aroon indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "AroonUp", ValueType = typeof(TOutput) },
            new() { Name = "AroonDown", ValueType = typeof(TOutput) },
            new() { Name = "AroonOscillator", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Aroon indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PAroon<TPrice, TOutput> p)
        => [
            new() { Name = "AroonUp", ValueType = typeof(TOutput) },
            new() { Name = "AroonDown", ValueType = typeof(TOutput) },
            new() { Name = "AroonOscillator", ValueType = typeof(TOutput) }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The Aroon parameters
    /// </summary>
    public readonly PAroon<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Aroon calculation
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
    /// Creates a new Aroon indicator instance
    /// </summary>
    public static Aroon_QC<TPrice, TOutput> Create(PAroon<TPrice, TOutput> p) => new Aroon_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Aroon indicator
    /// </summary>
    public Aroon_QC(PAroon<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.AroonOscillator(parameters.Period, parameters.Period))
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
    /// Gets the current Aroon Up value (0-100)
    /// </summary>
    public TOutput AroonUp => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.AroonUp.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Aroon Down value (0-100)
    /// </summary>
    public TOutput AroonDown => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.AroonDown.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current Aroon Oscillator value (-100 to +100)
    /// </summary>
    public TOutput AroonOscillator => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Indicates if the market is in a strong uptrend
    /// </summary>
    public bool IsUptrend 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(100) - Parameters.UptrendThreshold;
            return AroonUp > Parameters.UptrendThreshold && AroonDown < lowThreshold;
        }
    }

    /// <summary>
    /// Indicates if the market is in a strong downtrend
    /// </summary>
    public bool IsDowntrend 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(100) - Parameters.DowntrendThreshold;
            return AroonDown > Parameters.DowntrendThreshold && AroonUp < lowThreshold;
        }
    }

    /// <summary>
    /// Indicates if the market is consolidating
    /// </summary>
    public bool IsConsolidating 
    {
        get
        {
            if (!IsReady) return false;
            var lowThreshold = TOutput.CreateChecked(30);
            var highThreshold = TOutput.CreateChecked(70);
            return AroonUp >= lowThreshold && AroonUp <= highThreshold 
                && AroonDown >= lowThreshold && AroonDown <= highThreshold;
        }
    }

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
            // Create a TradeBar with High, Low, and Close values
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: "",
                open: Convert.ToDecimal(input.High), // Use High as Open for simplicity
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Low), // Use Low as Close for Aroon calculation
                volume: 0
            );

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output the Aroon values if ready
            if (WrappedIndicator.IsReady)
            {
                var aroonUp = ConvertToOutput(WrappedIndicator.AroonUp.Current.Price);
                var aroonDown = ConvertToOutput(WrappedIndicator.AroonDown.Current.Price);
                var aroonOscillator = ConvertToOutput(WrappedIndicator.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { aroonUp, aroonDown, aroonOscillator });
                }
                
                OnNext_PopulateOutput([aroonUp, aroonDown, aroonOscillator], output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                var defaultValue = default(TOutput)!;
                OnNext_PopulateOutput([defaultValue, defaultValue, defaultValue], output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Helper method to populate the output buffer
    /// </summary>
    private static void OnNext_PopulateOutput(TOutput[] values, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        foreach (var value in values)
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
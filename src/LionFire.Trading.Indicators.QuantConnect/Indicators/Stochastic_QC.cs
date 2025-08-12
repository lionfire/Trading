using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueWindows;
using QuantConnect.Data.Market;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Stochastic Oscillator indicator.
/// The Stochastic Oscillator is a momentum indicator that compares the closing price
/// to the range of prices over a period of time. It consists of two lines:
/// %K - the fast stochastic, and %D - the slow stochastic (signal line).
/// </summary>
public class Stochastic_QC<TPrice, TOutput> : QuantConnectIndicatorWrapper<Stochastic_QC<TPrice, TOutput>, global::QuantConnect.Indicators.Stochastic, PStochastic<TPrice, TOutput>, HLC<TPrice>, TOutput>, 
    IIndicator2<Stochastic_QC<TPrice, TOutput>, PStochastic<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, System.Numerics.INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Stochastic indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "%K",
                ValueType = typeof(TOutput),
                // Description = "Fast Stochastic value (0-100)"
            },
            new() {
                Name = "%D",
                ValueType = typeof(TOutput),
                // Description = "Slow Stochastic signal line (0-100)"
            }
        ];

    /// <summary>
    /// Gets the output slots for the Stochastic indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PStochastic<TPrice, TOutput> p)
        => [
            new() {
                Name = "%K",
                ValueType = typeof(TOutput),
                // Description = "Fast Stochastic value (0-100)"
            },
            new() {
                Name = "%D",
                ValueType = typeof(TOutput),
                // Description = "Slow Stochastic signal line (0-100)"
            }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The Stochastic parameters
    /// </summary>
    public readonly PStochastic<TPrice, TOutput> Parameters;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.FastPeriod + Parameters.SlowKPeriod + Parameters.SlowDPeriod;

    #endregion

    #endregion

    #region Lifecycle

    /// <summary>
    /// Creates a new Stochastic indicator instance
    /// </summary>
    public static Stochastic_QC<TPrice, TOutput> Create(PStochastic<TPrice, TOutput> p) => new Stochastic_QC<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Stochastic indicator
    /// </summary>
    public Stochastic_QC(PStochastic<TPrice, TOutput> parameters) : base(
        new global::QuantConnect.Indicators.Stochastic(
            parameters.FastPeriod,
            parameters.SlowKPeriod,
            parameters.SlowDPeriod))
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
    /// Gets the current %K value
    /// </summary>
    public TOutput PercentK => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.StochK.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Gets the current %D value (signal line)
    /// </summary>
    public TOutput PercentD => WrappedIndicator.IsReady ? ConvertToOutput(WrappedIndicator.StochD.Current.Price) : default(TOutput)!;

    /// <summary>
    /// Checks if the Stochastic indicates overbought conditions
    /// </summary>
    public bool IsOverbought => WrappedIndicator.IsReady && 
        Convert.ToDecimal(WrappedIndicator.StochK.Current.Price) >= Convert.ToDecimal(Parameters.OverboughtLevel);

    /// <summary>
    /// Checks if the Stochastic indicates oversold conditions
    /// </summary>
    public bool IsOversold => WrappedIndicator.IsReady && 
        Convert.ToDecimal(WrappedIndicator.StochK.Current.Price) <= Convert.ToDecimal(Parameters.OversoldLevel);

    #endregion

    #region Event Handling

    #region State

    // Stub time and period values. QuantConnect checks the symbol ID and increasing end times.
    static DateTime DefaultEndTime => new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    static TimeSpan period => new TimeSpan(0, 1, 0);

    DateTime endTime = DefaultEndTime;

    #endregion

    /// <summary>
    /// Process a batch of HLC price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Create a TradeBar with the HLC values for Stochastic calculation
            var tradeBar = new TradeBar(
                time: endTime,
                symbol: QuantConnect.Symbol.None,
                open: Convert.ToDecimal(input.High), // Open not used, but required
                high: Convert.ToDecimal(input.High),
                low: Convert.ToDecimal(input.Low),
                close: Convert.ToDecimal(input.Close),
                volume: 0,
                period: period);

            WrappedIndicator.Update(tradeBar);

            endTime += period;

            // Output both %K and %D values if ready
            if (WrappedIndicator.IsReady)
            {
                var kValue = ConvertToOutput(WrappedIndicator.StochK.Current.Price);
                var dValue = ConvertToOutput(WrappedIndicator.StochD.Current.Price);
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { kValue, dValue });
                }
                
                // Output %K
                OnNext_PopulateOutput(kValue, output, ref outputIndex, ref outputSkip);
                // Output %D
                OnNext_PopulateOutput(dValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
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
using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.IO;
using System.Numerics;
using QuantConnect.Indicators;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Ichimoku Cloud indicator.
/// Wraps QuantConnect's IchimokuKinkoHyo indicator for consistency.
/// </summary>
public class IchimokuCloud_QC<TPrice, TOutput> : IchimokuCloudBase<IchimokuCloud_QC<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<IchimokuCloud_QC<TPrice, TOutput>, PIchimokuCloud<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIchimokuCloud<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Creates an Ichimoku Cloud indicator instance
    /// </summary>
    public static IchimokuCloud_QC<TPrice, TOutput> Create(PIchimokuCloud<TPrice, TOutput> p) => new IchimokuCloud_QC<TPrice, TOutput>(p);

    #endregion

    #region Fields

    private readonly IchimokuKinkoHyo _ichimokuIndicator;
    private TOutput _tenkanSen;
    private TOutput _kijunSen;
    private TOutput _senkouSpanA;
    private TOutput _senkouSpanB;
    private TOutput _chikouSpan;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of the Ichimoku Cloud indicator
    /// </summary>
    public IchimokuCloud_QC(PIchimokuCloud<TPrice, TOutput> parameters) : base(parameters)
    {
        // Create QuantConnect's Ichimoku indicator with the parameters
        _ichimokuIndicator = new IchimokuKinkoHyo(
            tenkanPeriod: parameters.ConversionLinePeriod,
            kijunPeriod: parameters.BaseLinePeriod,
            senkouAPeriod: parameters.BaseLinePeriod, // Senkou A uses kijun period for averaging
            senkouBPeriod: parameters.LeadingSpanBPeriod,
            senkouADelayPeriod: parameters.Displacement,
            senkouBDelayPeriod: parameters.Displacement
        );
        
        // Initialize values
        _tenkanSen = TOutput.Zero;
        _kijunSen = TOutput.Zero;
        _senkouSpanA = TOutput.Zero;
        _senkouSpanB = TOutput.Zero;
        _chikouSpan = TOutput.Zero;
    }

    #endregion

    #region State

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => _ichimokuIndicator.IsReady;

    /// <summary>
    /// Tenkan-sen (Conversion Line)
    /// </summary>
    public override TOutput TenkanSen => _tenkanSen;

    /// <summary>
    /// Kijun-sen (Base Line)
    /// </summary>
    public override TOutput KijunSen => _kijunSen;

    /// <summary>
    /// Senkou Span A (Leading Span A) - with displacement
    /// </summary>
    public override TOutput SenkouSpanA => _senkouSpanA;

    /// <summary>
    /// Senkou Span B (Leading Span B) - with displacement
    /// </summary>
    public override TOutput SenkouSpanB => _senkouSpanB;

    /// <summary>
    /// Chikou Span (Lagging Span) - with displacement
    /// </summary>
    public override TOutput ChikouSpan => _chikouSpan;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert to QuantConnect's TradeBar format
            var tradeBar = new global::QuantConnect.Data.Market.TradeBar()
            {
                High = Convert.ToDecimal(input.High),
                Low = Convert.ToDecimal(input.Low),
                Close = Convert.ToDecimal(input.Close),
                Time = DateTime.UtcNow // This should ideally come from the input data
            };

            // Update the QuantConnect indicator
            _ichimokuIndicator.Update(tradeBar);

            // Extract the values and convert to our output type
            if (_ichimokuIndicator.IsReady)
            {
                _tenkanSen = ConvertFromDecimal(_ichimokuIndicator.Tenkan.Current.Value);
                _kijunSen = ConvertFromDecimal(_ichimokuIndicator.Kijun.Current.Value);
                _senkouSpanA = ConvertFromDecimal(_ichimokuIndicator.SenkouA.Current.Value);
                _senkouSpanB = ConvertFromDecimal(_ichimokuIndicator.SenkouB.Current.Value);
                _chikouSpan = ConvertFromDecimal(_ichimokuIndicator.Chikou.Current.Value);

                var outputs = new List<TOutput> { _tenkanSen, _kijunSen, _senkouSpanA, _senkouSpanB, _chikouSpan };
                
                if (subject != null)
                {
                    subject.OnNext(outputs);
                }
                
                OnNext_PopulateOutput(outputs, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default values while warming up
                var defaultOutputs = new List<TOutput> { TOutput.Zero, TOutput.Zero, TOutput.Zero, TOutput.Zero, TOutput.Zero };
                OnNext_PopulateOutput(defaultOutputs, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Helper method to populate the output buffer
    /// </summary>
    private static void OnNext_PopulateOutput(List<TOutput> values, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) 
        { 
            outputSkip--; 
        }
        else if (outputBuffer != null) 
        {
            // For multi-output indicators, we typically store the first output or create a composite
            // For simplicity, we'll store the Tenkan-sen (first value)
            if (values.Count > 0)
            {
                outputBuffer[outputIndex++] = values[0];
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Convert from decimal to output type
    /// </summary>
    private static TOutput ConvertFromDecimal(decimal value)
    {
        // Handle common conversions efficiently
        if (typeof(TOutput) == typeof(decimal))
        {
            return (TOutput)(object)value;
        }
        else if (typeof(TOutput) == typeof(double))
        {
            return (TOutput)(object)(double)value;
        }
        else if (typeof(TOutput) == typeof(float))
        {
            return (TOutput)(object)(float)value;
        }
        else
        {
            // Use generic conversion for other types
            return TOutput.CreateChecked(Convert.ToDouble(value));
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
        
        _ichimokuIndicator.Reset();
        
        _tenkanSen = TOutput.Zero;
        _kijunSen = TOutput.Zero;
        _senkouSpanA = TOutput.Zero;
        _senkouSpanB = TOutput.Zero;
        _chikouSpan = TOutput.Zero;
    }

    #endregion
}
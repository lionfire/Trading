using LionFire.Structures;
using LionFire.Trading;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using CircularBuffer;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Ichimoku Cloud indicator.
/// Uses circular buffers for efficient high/low tracking over multiple periods.
/// </summary>
public class IchimokuCloud_FP<TPrice, TOutput> : IchimokuCloudBase<IchimokuCloud_FP<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<IchimokuCloud_FP<TPrice, TOutput>, PIchimokuCloud<TPrice, TOutput>, HLC<TPrice>, TOutput>,
    IIchimokuCloud<HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Creates an Ichimoku Cloud indicator instance
    /// </summary>
    public static IchimokuCloud_FP<TPrice, TOutput> Create(PIchimokuCloud<TPrice, TOutput> p) => new IchimokuCloud_FP<TPrice, TOutput>(p);

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of the Ichimoku Cloud indicator
    /// </summary>
    public IchimokuCloud_FP(PIchimokuCloud<TPrice, TOutput> parameters) : base(parameters)
    {
        // Initialize circular buffers for high/low tracking
        int maxPeriod = Math.Max(Math.Max(ConversionLinePeriod, BaseLinePeriod), LeadingSpanBPeriod);
        
        highBuffer = new CircularBuffer<TOutput>(maxPeriod);
        lowBuffer = new CircularBuffer<TOutput>(maxPeriod);
        
        // Buffer for close prices (for Chikou Span)
        closeBuffer = new CircularBuffer<TOutput>(Displacement + 1);
        
        // Buffers for leading spans displacement  
        senkouSpanABuffer = new CircularBuffer<TOutput>(Displacement);
        senkouSpanBBuffer = new CircularBuffer<TOutput>(Displacement);
        
        // Initialize state
        samplesProcessed = 0;
        tenkanSen = TOutput.Zero;
        kijunSen = TOutput.Zero;
        senkouSpanA = TOutput.Zero;
        senkouSpanB = TOutput.Zero;
        chikouSpan = TOutput.Zero;
    }

    #endregion

    #region State

    private readonly CircularBuffer<TOutput> highBuffer;
    private readonly CircularBuffer<TOutput> lowBuffer;
    private readonly CircularBuffer<TOutput> closeBuffer;
    private readonly CircularBuffer<TOutput> senkouSpanABuffer;
    private readonly CircularBuffer<TOutput> senkouSpanBBuffer;
    
    private int samplesProcessed;
    
    private TOutput tenkanSen;
    private TOutput kijunSen;
    private TOutput senkouSpanA;
    private TOutput senkouSpanB;
    private TOutput chikouSpan;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce basic values
    /// </summary>
    public override bool IsReady => samplesProcessed >= Math.Max(BaseLinePeriod, LeadingSpanBPeriod);

    /// <summary>
    /// Tenkan-sen (Conversion Line)
    /// </summary>
    public override TOutput TenkanSen => tenkanSen;

    /// <summary>
    /// Kijun-sen (Base Line)
    /// </summary>
    public override TOutput KijunSen => kijunSen;

    /// <summary>
    /// Senkou Span A (Leading Span A) - with displacement
    /// </summary>
    public override TOutput SenkouSpanA => senkouSpanA;

    /// <summary>
    /// Senkou Span B (Leading Span B) - with displacement
    /// </summary>
    public override TOutput SenkouSpanB => senkouSpanB;

    /// <summary>
    /// Chikou Span (Lagging Span) - with displacement
    /// </summary>
    public override TOutput ChikouSpan => chikouSpan;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of HLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput
            TOutput high = ConvertToOutput(input.High);
            TOutput low = ConvertToOutput(input.Low);
            TOutput close = ConvertToOutput(input.Close);

            // Store in circular buffers
            highBuffer.PushBack(high);
            lowBuffer.PushBack(low);
            closeBuffer.PushBack(close);
            
            samplesProcessed++;

            // Calculate Tenkan-sen (Conversion Line)
            if (samplesProcessed >= ConversionLinePeriod)
            {
                tenkanSen = CalculateHighLowAverage(ConversionLinePeriod);
            }

            // Calculate Kijun-sen (Base Line)
            if (samplesProcessed >= BaseLinePeriod)
            {
                kijunSen = CalculateHighLowAverage(BaseLinePeriod);
            }

            // Calculate current Senkou Span A (before displacement)
            TOutput currentSenkouSpanA = TOutput.Zero;
            if (samplesProcessed >= Math.Max(ConversionLinePeriod, BaseLinePeriod))
            {
                currentSenkouSpanA = (tenkanSen + kijunSen) / TOutput.CreateChecked(2);
                senkouSpanABuffer.PushBack(currentSenkouSpanA);
            }

            // Calculate current Senkou Span B (before displacement)
            TOutput currentSenkouSpanB = TOutput.Zero;
            if (samplesProcessed >= LeadingSpanBPeriod)
            {
                currentSenkouSpanB = CalculateHighLowAverage(LeadingSpanBPeriod);
                senkouSpanBBuffer.PushBack(currentSenkouSpanB);
            }

            // Apply displacement for Senkou Spans (leading)
            if (senkouSpanABuffer.Size >= Displacement)
            {
                senkouSpanA = senkouSpanABuffer[0]; // Oldest value (26 periods ago)
            }

            if (senkouSpanBBuffer.Size >= Displacement)
            {
                senkouSpanB = senkouSpanBBuffer[0]; // Oldest value (26 periods ago)
            }

            // Apply displacement for Chikou Span (lagging)
            // For Chikou Span, we want the current close price but it's plotted 26 periods behind
            // So when we're calculating current values, Chikou Span shows the close from 26 periods ahead perspective
            chikouSpan = close; // Current close is what will be the Chikou Span value

            // Populate outputs if ready
            if (IsReady)
            {
                var outputs = new List<TOutput> { tenkanSen, kijunSen, senkouSpanA, senkouSpanB, chikouSpan };
                
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
    /// Calculates the average of highest high and lowest low over the specified period
    /// </summary>
    private TOutput CalculateHighLowAverage(int period)
    {
        if (highBuffer.Size < period || lowBuffer.Size < period)
        {
            return TOutput.Zero;
        }

        // Find highest high and lowest low over the period
        TOutput highestHigh = highBuffer[highBuffer.Size - period];
        TOutput lowestLow = lowBuffer[lowBuffer.Size - period];
        
        for (int i = highBuffer.Size - period + 1; i < highBuffer.Size; i++)
        {
            if (highBuffer[i] > highestHigh)
                highestHigh = highBuffer[i];
            if (lowBuffer[i] < lowestLow)
                lowestLow = lowBuffer[i];
        }

        return (highestHigh + lowestLow) / TOutput.CreateChecked(2);
    }

    /// <summary>
    /// Convert input type to output type
    /// </summary>
    private static TOutput ConvertToOutput(TPrice input)
    {
        // Handle common conversions efficiently
        if (typeof(TPrice) == typeof(TOutput))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(double) && typeof(TOutput) == typeof(double))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(float) && typeof(TOutput) == typeof(float))
        {
            return (TOutput)(object)input;
        }
        else if (typeof(TPrice) == typeof(decimal) && typeof(TOutput) == typeof(decimal))
        {
            return (TOutput)(object)input;
        }
        else
        {
            // Use generic conversion for other types
            return TOutput.CreateChecked(Convert.ToDouble(input));
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
        
        highBuffer.Clear();
        lowBuffer.Clear();
        closeBuffer.Clear();
        senkouSpanABuffer.Clear();
        senkouSpanBBuffer.Clear();
        
        samplesProcessed = 0;
        tenkanSen = TOutput.Zero;
        kijunSen = TOutput.Zero;
        senkouSpanA = TOutput.Zero;
        senkouSpanB = TOutput.Zero;
        chikouSpan = TOutput.Zero;
    }

    #endregion
}
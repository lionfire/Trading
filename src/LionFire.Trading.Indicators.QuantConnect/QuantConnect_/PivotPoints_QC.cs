using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Pivot Points
/// Wraps the QuantConnect PivotPointsHighLowClose indicator (if available) with LionFire interfaces
/// Falls back to first-party implementation if QuantConnect version is not available
/// </summary>
public class PivotPoints_QC<TInput, TOutput> 
    : PivotPointsBase<TInput, TOutput>
    , IIndicator2<PivotPoints_QC<TInput, TOutput>, PPivotPoints<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly dynamic? qcIndicator;
    private readonly bool useQuantConnect;
    private readonly Native.PivotPoints_FP<TInput, TOutput>? fallbackIndicator;
    
    // Current pivot values
    private TOutput currentPivotPoint;
    private TOutput currentR1;
    private TOutput currentS1;
    private TOutput currentR2;
    private TOutput currentS2;
    private TOutput currentR3;
    private TOutput currentS3;
    
    private int dataPointsReceived = 0;
    private bool hasValidData = false;

    #endregion

    #region Properties

    public override TOutput PivotPoint => hasValidData ? currentPivotPoint : MissingOutputValue;
    public override TOutput Resistance1 => hasValidData ? currentR1 : MissingOutputValue;
    public override TOutput Support1 => hasValidData ? currentS1 : MissingOutputValue;
    public override TOutput Resistance2 => hasValidData ? currentR2 : MissingOutputValue;
    public override TOutput Support2 => hasValidData ? currentS2 : MissingOutputValue;
    public override TOutput Resistance3 => hasValidData ? currentR3 : MissingOutputValue;
    public override TOutput Support3 => hasValidData ? currentS3 : MissingOutputValue;

    public override bool IsReady => useQuantConnect ? (qcIndicator?.IsReady ?? false) : (fallbackIndicator?.IsReady ?? false);

    #endregion

    #region Lifecycle

    public static PivotPoints_QC<TInput, TOutput> Create(PPivotPoints<TInput, TOutput> p)
        => new PivotPoints_QC<TInput, TOutput>(p);

    public PivotPoints_QC(PPivotPoints<TInput, TOutput> parameters) : base(parameters)
    {
        try
        {
            // Try to create QuantConnect PivotPointsHighLowClose indicator
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var pivotType = qcAssembly.GetType("QuantConnect.Indicators.PivotPointsHighLowClose");
            
            if (pivotType != null)
            {
                // QuantConnect PivotPointsHighLowClose constructor
                qcIndicator = Activator.CreateInstance(pivotType, $"PivotPoints({parameters.PeriodType})");
                useQuantConnect = true;
            }
            else
            {
                // QuantConnect indicator not found, use fallback
                useQuantConnect = false;
                fallbackIndicator = new Native.PivotPoints_FP<TInput, TOutput>(parameters);
            }
        }
        catch
        {
            // Failed to load QuantConnect, use fallback
            useQuantConnect = false;
            fallbackIndicator = new Native.PivotPoints_FP<TInput, TOutput>(parameters);
        }
        
        // Initialize current values
        InitializeValues();
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        if (useQuantConnect && qcIndicator != null)
        {
            HandleQuantConnectUpdate(inputs, output, ref outputIndex, ref outputSkip);
        }
        else if (fallbackIndicator != null)
        {
            // Use fallback first-party implementation
            fallbackIndicator.OnBarBatch(inputs, output, outputIndex, outputSkip);
            
            // Copy values from fallback
            currentPivotPoint = fallbackIndicator.PivotPoint;
            currentR1 = fallbackIndicator.Resistance1;
            currentS1 = fallbackIndicator.Support1;
            currentR2 = fallbackIndicator.Resistance2;
            currentS2 = fallbackIndicator.Support2;
            currentR3 = fallbackIndicator.Resistance3;
            currentS3 = fallbackIndicator.Support3;
            hasValidData = fallbackIndicator.IsReady;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Handle updates using QuantConnect indicator
    /// </summary>
    private void HandleQuantConnectUpdate(IReadOnlyList<TInput> inputs, TOutput[]? output, ref int outputIndex, ref int outputSkip)
    {
        foreach (var input in inputs)
        {
            try
            {
                // Extract OHLC data
                var (open, high, low, close) = ExtractOHLC(input);
                
                // Create QuantConnect TradeBar-like object
                var qcBar = CreateQuantConnectBar(
                    ConvertToDecimal(open),
                    ConvertToDecimal(high),
                    ConvertToDecimal(low),
                    ConvertToDecimal(close)
                );
                
                // Update the QuantConnect indicator
                qcIndicator.Update(qcBar);
                
                dataPointsReceived++;
                
                // Get the results and convert to TOutput
                if (qcIndicator.IsReady)
                {
                    // Extract pivot point values from QuantConnect indicator
                    // Note: Actual property names may vary depending on QuantConnect implementation
                    currentPivotPoint = TOutput.CreateChecked((decimal)qcIndicator.PivotPoint);
                    currentR1 = TOutput.CreateChecked((decimal)qcIndicator.R1);
                    currentS1 = TOutput.CreateChecked((decimal)qcIndicator.S1);
                    currentR2 = TOutput.CreateChecked((decimal)qcIndicator.R2);
                    currentS2 = TOutput.CreateChecked((decimal)qcIndicator.S2);
                    currentR3 = TOutput.CreateChecked((decimal)qcIndicator.R3);
                    currentS3 = TOutput.CreateChecked((decimal)qcIndicator.S3);
                    hasValidData = true;
                }
                
                // Output all 7 pivot values
                OutputCurrentValues(output, ref outputIndex, ref outputSkip);
                
                // Notify observers
                if (subject != null && IsReady)
                {
                    var results = new[] { currentPivotPoint, currentR1, currentS1, currentR2, currentS2, currentR3, currentS3 };
                    subject.OnNext(results);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing bar in PivotPoints_QC: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Convert input type to decimal for QuantConnect processing
    /// </summary>
    private static decimal ConvertToDecimal(TOutput input)
    {
        return input switch
        {
            decimal d => d,
            double db => (decimal)db,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => Convert.ToDecimal(input)
        };
    }

    /// <summary>
    /// Create a QuantConnect-compatible OHLC bar object
    /// </summary>
    private dynamic CreateQuantConnectBar(decimal open, decimal high, decimal low, decimal close)
    {
        try
        {
            // Try to create a QuantConnect TradeBar
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Common");
            var tradeBarType = qcAssembly.GetType("QuantConnect.Data.Market.TradeBar");
            
            if (tradeBarType != null)
            {
                var now = DateTime.UtcNow;
                var timespan = TimeSpan.FromDays(1); // Assume daily bars
                
                return Activator.CreateInstance(tradeBarType, 
                    now, "PIVOT", open, high, low, close, 0m); // volume = 0
            }
            
            // Fallback: create anonymous object with required properties
            return new 
            { 
                Time = DateTime.UtcNow,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 0m
            };
        }
        catch
        {
            // Final fallback: simple anonymous object with OHLC
            return new 
            { 
                Time = DateTime.UtcNow,
                Open = open,
                High = high,
                Low = low,
                Close = close
            };
        }
    }

    /// <summary>
    /// Initialize pivot values to missing
    /// </summary>
    private void InitializeValues()
    {
        currentPivotPoint = MissingOutputValue;
        currentR1 = MissingOutputValue;
        currentS1 = MissingOutputValue;
        currentR2 = MissingOutputValue;
        currentS2 = MissingOutputValue;
        currentR3 = MissingOutputValue;
        currentS3 = MissingOutputValue;
    }

    /// <summary>
    /// Helper method to output current pivot point values to the output buffer
    /// </summary>
    private void OutputCurrentValues(TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        var values = new[] { PivotPoint, Resistance1, Support1, Resistance2, Support2, Resistance3, Support3 };
        
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

    #region Cleanup

    public override void Clear()
    {
        qcIndicator?.Reset();
        fallbackIndicator?.Clear();
        
        InitializeValues();
        dataPointsReceived = 0;
        hasValidData = false;
    }

    #endregion
}
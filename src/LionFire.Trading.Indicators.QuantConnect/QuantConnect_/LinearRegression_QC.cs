using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Linear Regression
/// Wraps the QuantConnect LinearRegressionChannel or similar indicator with LionFire interfaces
/// </summary>
public class LinearRegression_QC<TPrice, TOutput> 
    : LinearRegressionBase<LinearRegression_QC<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<LinearRegression_QC<TPrice, TOutput>, PLinearRegression<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly dynamic? qcIndicator;
    private TOutput currentValue;
    private TOutput currentSlope;
    private TOutput currentIntercept;
    private TOutput currentRSquared;
    private int dataPointsReceived = 0;
    
    // Fallback to manual calculation if QC not available
    private readonly bool useManualCalculation;
    private readonly TOutput[] buffer;
    private int bufferIndex;
    private int count;

    #endregion

    #region Properties

    public override TOutput Value => IsReady ? currentValue : default(TOutput)!;
    
    public override TOutput Slope => IsReady ? currentSlope : default(TOutput)!;
    
    public override TOutput Intercept => IsReady ? currentIntercept : default(TOutput)!;
    
    public override TOutput RSquared => IsReady ? currentRSquared : default(TOutput)!;
    
    public override bool IsReady => dataPointsReceived >= MaxLookback;

    #endregion

    #region Lifecycle

    public static LinearRegression_QC<TPrice, TOutput> Create(PLinearRegression<TPrice, TOutput> p)
        => new LinearRegression_QC<TPrice, TOutput>(p);

    public LinearRegression_QC(PLinearRegression<TPrice, TOutput> parameters) : base(parameters)
    {
        try
        {
            // Try to create QuantConnect LinearRegressionChannel or similar indicator
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var lrcType = qcAssembly.GetType("QuantConnect.Indicators.LinearRegressionChannel");
            
            if (lrcType != null)
            {
                qcIndicator = Activator.CreateInstance(lrcType, 
                    $"LinearRegression({parameters.Period})", 
                    parameters.Period);
                useManualCalculation = false;
            }
            else
            {
                // Fallback: use manual calculation
                useManualCalculation = true;
                buffer = new TOutput[parameters.Period];
                bufferIndex = 0;
                count = 0;
            }
        }
        catch
        {
            // Fallback: use manual calculation if QuantConnect is not available
            useManualCalculation = true;
            buffer = new TOutput[parameters.Period];
            bufferIndex = 0;
            count = 0;
        }
        
        currentValue = default(TOutput)!;
        currentSlope = default(TOutput)!;
        currentIntercept = default(TOutput)!;
        currentRSquared = default(TOutput)!;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            try
            {
                if (!useManualCalculation && qcIndicator != null)
                {
                    ProcessWithQuantConnect(input);
                }
                else
                {
                    ProcessWithManualCalculation(input);
                }
                
                dataPointsReceived++;
                
                // Output values
                var outputValue = IsReady ? currentValue : MissingOutputValue;
                
                if (outputSkip > 0)
                {
                    outputSkip--;
                }
                else if (output != null && outputIndex < output.Length)
                {
                    output[outputIndex++] = outputValue;
                }
                
                // Notify observers
                if (subject != null && IsReady)
                {
                    subject.OnNext(new List<TOutput> { currentValue, currentSlope, currentIntercept, currentRSquared });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing input in LinearRegression_QC: {ex.Message}", ex);
            }
        }
    }

    private void ProcessWithQuantConnect(TPrice input)
    {
        if (qcIndicator == null) return;
        
        // Convert input to decimal for QuantConnect processing
        var inputValue = ConvertToDecimal(input);
        
        // Create IBaseData-like object for QuantConnect indicator
        var qcDataPoint = CreateQuantConnectDataPoint(inputValue);
        
        // Update the QuantConnect indicator
        qcIndicator.Update(qcDataPoint);
        
        // Extract values from QuantConnect indicator
        if (qcIndicator.IsReady)
        {
            try
            {
                // Try to get regression line value
                var qcValue = (decimal)qcIndicator.Current.Value;
                currentValue = TOutput.CreateChecked(qcValue);
                
                // Try to extract slope and intercept if available
                // Note: QuantConnect LinearRegressionChannel may have different properties
                if (HasProperty(qcIndicator, "Slope"))
                {
                    var qcSlope = (decimal)qcIndicator.Slope;
                    currentSlope = TOutput.CreateChecked(qcSlope);
                }
                
                if (HasProperty(qcIndicator, "Intercept"))
                {
                    var qcIntercept = (decimal)qcIndicator.Intercept;
                    currentIntercept = TOutput.CreateChecked(qcIntercept);
                }
                
                // R-squared might not be available in QC, set to zero or calculate manually
                currentRSquared = TOutput.Zero;
            }
            catch
            {
                // Fall back to manual calculation if extraction fails
                ProcessWithManualCalculation(input);
            }
        }
    }

    private void ProcessWithManualCalculation(TPrice input)
    {
        TOutput newValue = ConvertToOutput(input);
        
        // Simple manual linear regression calculation for fallback
        if (count >= Period)
        {
            // Shift buffer
            for (int i = 0; i < Period - 1; i++)
            {
                buffer[i] = buffer[i + 1];
            }
            buffer[Period - 1] = newValue;
        }
        else
        {
            buffer[count] = newValue;
            count++;
        }
        
        if (count >= Period)
        {
            CalculateLinearRegression();
        }
    }

    private void CalculateLinearRegression()
    {
        TOutput n = TOutput.CreateChecked(Period);
        TOutput sumX = TOutput.Zero;
        TOutput sumY = TOutput.Zero;
        TOutput sumXY = TOutput.Zero;
        TOutput sumXSquared = TOutput.Zero;
        TOutput sumYSquared = TOutput.Zero;
        
        for (int i = 0; i < Period; i++)
        {
            TOutput x = TOutput.CreateChecked(i);
            TOutput y = buffer[i];
            
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumXSquared += x * x;
            sumYSquared += y * y;
        }
        
        // Calculate slope: slope = (n*Σ(xy) - Σx*Σy) / (n*Σ(x²) - (Σx)²)
        TOutput numerator = n * sumXY - sumX * sumY;
        TOutput denominator = n * sumXSquared - sumX * sumX;
        
        if (denominator != TOutput.Zero)
        {
            currentSlope = numerator / denominator;
            currentIntercept = (sumY - currentSlope * sumX) / n;
            
            // Current regression value at the last point
            TOutput lastX = TOutput.CreateChecked(Period - 1);
            currentValue = currentSlope * lastX + currentIntercept;
            
            // Calculate R-squared
            TOutput meanY = sumY / n;
            TOutput tss = TOutput.Zero;
            TOutput rss = TOutput.Zero;
            
            for (int i = 0; i < Period; i++)
            {
                TOutput x = TOutput.CreateChecked(i);
                TOutput y = buffer[i];
                TOutput yPredicted = currentSlope * x + currentIntercept;
                
                TOutput diffFromMean = y - meanY;
                tss += diffFromMean * diffFromMean;
                
                TOutput diffFromPredicted = y - yPredicted;
                rss += diffFromPredicted * diffFromPredicted;
            }
            
            if (tss != TOutput.Zero)
            {
                currentRSquared = TOutput.One - (rss / tss);
            }
            else
            {
                currentRSquared = TOutput.One;
            }
            
            // Clamp R-squared to [0, 1]
            if (currentRSquared < TOutput.Zero) currentRSquared = TOutput.Zero;
            if (currentRSquared > TOutput.One) currentRSquared = TOutput.One;
        }
        else
        {
            currentSlope = TOutput.Zero;
            currentIntercept = sumY / n;
            currentValue = currentIntercept;
            currentRSquared = TOutput.Zero;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Convert input type to decimal for QuantConnect processing
    /// </summary>
    private static decimal ConvertToDecimal(TPrice input)
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

    /// <summary>
    /// Create a QuantConnect-compatible data point object
    /// </summary>
    private dynamic CreateQuantConnectDataPoint(decimal value)
    {
        try
        {
            // Create a simple data point for the QuantConnect indicator
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Common");
            var indicatorDataPointType = qcAssembly.GetType("QuantConnect.Data.Market.IndicatorDataPoint");
            
            if (indicatorDataPointType != null)
            {
                var now = DateTime.UtcNow;
                return Activator.CreateInstance(indicatorDataPointType, now, value);
            }
            
            // Fallback: create anonymous object with required properties
            return new { Time = DateTime.UtcNow, Value = value };
        }
        catch
        {
            // Final fallback: simple anonymous object
            return new { Time = DateTime.UtcNow, Value = value };
        }
    }

    /// <summary>
    /// Check if a dynamic object has a specific property
    /// </summary>
    private static bool HasProperty(dynamic obj, string propertyName)
    {
        try
        {
            var type = obj.GetType();
            return type.GetProperty(propertyName) != null;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Cleanup

    public override void Clear()
    {
        base.Clear();
        qcIndicator?.Reset();
        currentValue = default(TOutput)!;
        currentSlope = default(TOutput)!;
        currentIntercept = default(TOutput)!;
        currentRSquared = default(TOutput)!;
        dataPointsReceived = 0;
        
        if (useManualCalculation)
        {
            if (buffer != null)
                Array.Clear(buffer, 0, buffer.Length);
            bufferIndex = 0;
            count = 0;
        }
    }

    #endregion
}
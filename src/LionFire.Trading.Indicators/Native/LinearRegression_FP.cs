using LionFire.Structures;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Linear Regression indicator.
/// Uses efficient incremental calculation with least squares method for O(1) computation.
/// </summary>
public class LinearRegression_FP<TPrice, TOutput> : SingleInputIndicatorBase<LinearRegression_FP<TPrice, TOutput>, PLinearRegression<TPrice, TOutput>, TPrice, TOutput>,
    IIndicator2<LinearRegression_FP<TPrice, TOutput>, PLinearRegression<TPrice, TOutput>, TPrice, TOutput>,
    ILinearRegression<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Linear Regression indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "Value",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Slope",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Intercept",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "RSquared",
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Linear Regression indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PLinearRegression<TPrice, TOutput> p)
        => [
            new() {
                Name = "Value",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Slope",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Intercept",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "RSquared",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Parameters

    /// <summary>
    /// The Linear Regression parameters
    /// </summary>
    public readonly PLinearRegression<TPrice, TOutput> Parameters;

    /// <summary>
    /// The period used for Linear Regression calculation
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
    /// Creates a new Linear Regression indicator instance
    /// </summary>
    public static LinearRegression_FP<TPrice, TOutput> Create(PLinearRegression<TPrice, TOutput> p) => new LinearRegression_FP<TPrice, TOutput>(p);

    /// <summary>
    /// Initializes a new instance of the Linear Regression indicator
    /// </summary>
    public LinearRegression_FP(PLinearRegression<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        buffer = new TOutput[parameters.Period];
        bufferIndex = 0;
        count = 0;
        
        // Pre-calculate sum of x values for efficiency: 0, 1, 2, ..., (n-1)
        sumX = TOutput.CreateChecked((parameters.Period - 1) * parameters.Period / 2);
        
        // Pre-calculate sum of x squared values: 0² + 1² + 2² + ... + (n-1)²
        sumXSquared = TOutput.CreateChecked((parameters.Period - 1) * parameters.Period * (2 * parameters.Period - 1) / 6);
        
        sumY = TOutput.Zero;
        sumXY = TOutput.Zero;
        sumYSquared = TOutput.Zero;
        
        currentValue = TOutput.Zero;
        currentSlope = TOutput.Zero;
        currentIntercept = TOutput.Zero;
        currentRSquared = TOutput.Zero;
    }

    #endregion

    #region State

    private readonly TOutput[] buffer;
    private int bufferIndex;
    private int count;
    
    // Pre-calculated constants for efficiency
    private readonly TOutput sumX;
    private readonly TOutput sumXSquared;
    
    // Running sums for incremental calculation
    private TOutput sumY;
    private TOutput sumXY;
    private TOutput sumYSquared;
    
    // Current calculated values
    private TOutput currentValue;
    private TOutput currentSlope;
    private TOutput currentIntercept;
    private TOutput currentRSquared;

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public override bool IsReady => count >= Period;

    /// <summary>
    /// Gets the current regression value (current point on regression line)
    /// </summary>
    public TOutput Value => IsReady ? currentValue : default(TOutput)!;

    /// <summary>
    /// Gets the current slope of the regression line (rate of change)
    /// </summary>
    public TOutput Slope => IsReady ? currentSlope : default(TOutput)!;

    /// <summary>
    /// Gets the current intercept of the regression line
    /// </summary>
    public TOutput Intercept => IsReady ? currentIntercept : default(TOutput)!;

    /// <summary>
    /// Gets the current R-squared value (coefficient of determination)
    /// </summary>
    public TOutput RSquared => IsReady ? currentRSquared : default(TOutput)!;

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of price inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput
            TOutput newValue = ConvertToOutput(input);

            // Handle buffer full case - remove oldest value from sums
            if (count >= Period)
            {
                TOutput oldValue = buffer[bufferIndex];
                
                // Remove old value contributions from running sums
                sumY -= oldValue;
                sumYSquared -= oldValue * oldValue;
                
                // Remove old XY contribution (oldest value was at x position 0, now all shift left)
                // We need to recalculate sumXY when sliding window
                RecalculateSumXY();
            }

            // Add new value to buffer and sums
            buffer[bufferIndex] = newValue;
            sumY += newValue;
            sumYSquared += newValue * newValue;

            // Move to next position in circular buffer
            bufferIndex = (bufferIndex + 1) % Period;

            // Increment count until we reach the period
            if (count < Period)
            {
                count++;
            }

            // Calculate regression if we have enough data
            if (IsReady)
            {
                CalculateRegression();
                
                if (subject != null)
                {
                    subject.OnNext(new List<TOutput> { currentValue, currentSlope, currentIntercept, currentRSquared });
                }
                
                OnNext_PopulateOutput(currentValue, output, ref outputIndex, ref outputSkip);
            }
            else
            {
                // Output default value while warming up
                OnNext_PopulateOutput(default(TOutput)!, output, ref outputIndex, ref outputSkip);
            }
        }
    }

    /// <summary>
    /// Recalculates the sum of XY products for the sliding window
    /// </summary>
    private void RecalculateSumXY()
    {
        sumXY = TOutput.Zero;
        
        for (int i = 0; i < Period; i++)
        {
            // Get the value at position i in the buffer (considering circular nature)
            int actualIndex = (bufferIndex - Period + i + Period) % Period;
            TOutput y = buffer[actualIndex];
            TOutput x = TOutput.CreateChecked(i);
            sumXY += x * y;
        }
    }

    /// <summary>
    /// Calculate linear regression using least squares method
    /// </summary>
    private void CalculateRegression()
    {
        TOutput n = TOutput.CreateChecked(Period);
        
        // Calculate slope: slope = (n*Σ(xy) - Σx*Σy) / (n*Σ(x²) - (Σx)²)
        TOutput numerator = n * sumXY - sumX * sumY;
        TOutput denominator = n * sumXSquared - sumX * sumX;
        
        if (denominator != TOutput.Zero)
        {
            currentSlope = numerator / denominator;
            
            // Calculate intercept: intercept = (Σy - slope*Σx) / n
            currentIntercept = (sumY - currentSlope * sumX) / n;
            
            // Calculate current regression value at the last point (x = Period - 1)
            TOutput lastX = TOutput.CreateChecked(Period - 1);
            currentValue = currentSlope * lastX + currentIntercept;
            
            // Calculate R-squared
            CalculateRSquared();
        }
        else
        {
            // Vertical line case or all points are the same
            currentSlope = TOutput.Zero;
            currentIntercept = sumY / n;
            currentValue = currentIntercept;
            currentRSquared = TOutput.Zero;
        }
    }

    /// <summary>
    /// Calculate R-squared (coefficient of determination)
    /// </summary>
    private void CalculateRSquared()
    {
        TOutput n = TOutput.CreateChecked(Period);
        TOutput meanY = sumY / n;
        
        // Calculate total sum of squares (TSS) and residual sum of squares (RSS)
        TOutput tss = TOutput.Zero;
        TOutput rss = TOutput.Zero;
        
        for (int i = 0; i < Period; i++)
        {
            int actualIndex = (bufferIndex - Period + i + Period) % Period;
            TOutput y = buffer[actualIndex];
            TOutput x = TOutput.CreateChecked(i);
            
            // Predicted value at this point
            TOutput yPredicted = currentSlope * x + currentIntercept;
            
            // TSS = Σ(y - mean_y)²
            TOutput diffFromMean = y - meanY;
            tss += diffFromMean * diffFromMean;
            
            // RSS = Σ(y - y_predicted)²
            TOutput diffFromPredicted = y - yPredicted;
            rss += diffFromPredicted * diffFromPredicted;
        }
        
        // R² = 1 - (RSS / TSS)
        if (tss != TOutput.Zero)
        {
            currentRSquared = TOutput.One - (rss / tss);
        }
        else
        {
            currentRSquared = TOutput.One; // Perfect fit when all values are the same
        }
        
        // Clamp R-squared to [0, 1] range in case of numerical issues
        if (currentRSquared < TOutput.Zero)
            currentRSquared = TOutput.Zero;
        if (currentRSquared > TOutput.One)
            currentRSquared = TOutput.One;
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
        Array.Clear(buffer, 0, buffer.Length);
        bufferIndex = 0;
        count = 0;
        sumY = TOutput.Zero;
        sumXY = TOutput.Zero;
        sumYSquared = TOutput.Zero;
        currentValue = TOutput.Zero;
        currentSlope = TOutput.Zero;
        currentIntercept = TOutput.Zero;
        currentRSquared = TOutput.Zero;
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
}
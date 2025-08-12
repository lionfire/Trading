using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Linear Regression indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class LinearRegression
{
    /// <summary>
    /// Creates a Linear Regression indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Linear Regression parameters</param>
    /// <returns>A Linear Regression indicator instance</returns>
    public static ILinearRegression<TPrice, TOutput> Create<TPrice, TOutput>(PLinearRegression<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new LinearRegression_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new LinearRegression_FP<TPrice, TOutput>(parameters), // FP is already optimized with incremental calculation
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Linear Regression indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Linear Regression calculation (default: 14)</param>
    /// <returns>A Linear Regression indicator instance</returns>
    public static ILinearRegression<TPrice, TOutput> Create<TPrice, TOutput>(int period = 14)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PLinearRegression<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static ILinearRegression<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PLinearRegression<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators");
            var lrqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.LinearRegression_QC`2");
            
            if (lrqcType != null)
            {
                // Make the generic type
                var genericType = lrqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is ILinearRegression<TPrice, TOutput> linearRegression)
                {
                    return linearRegression;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new LinearRegression_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static ILinearRegression<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PLinearRegression<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - For small to medium periods (< 100), the first-party implementation with optimized calculation is very efficient
        // - For larger periods or when QuantConnect is already loaded, use QuantConnect
        // - Default to first-party for its efficiency and comprehensive feature set (slope, intercept, R-squared)

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true ||
                      a.FullName?.StartsWith("QuantConnect") == true);

        if (quantConnectLoaded && parameters.Period > 100)
        {
            // Use QuantConnect for very large periods when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency and feature completeness
            return new LinearRegression_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a Linear Regression indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Linear Regression calculation</param>
    /// <returns>A Linear Regression indicator instance for double values</returns>
    public static ILinearRegression<double, double> CreateDouble(int period = 14)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create a Linear Regression indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Linear Regression calculation</param>
    /// <returns>A Linear Regression indicator instance for decimal values</returns>
    public static ILinearRegression<decimal, decimal> CreateDecimal(int period = 14)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a Linear Regression indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Linear Regression calculation</param>
    /// <returns>A Linear Regression indicator instance for float values</returns>
    public static ILinearRegression<float, float> CreateFloat(int period = 14)
    {
        return Create<float, float>(period);
    }
}
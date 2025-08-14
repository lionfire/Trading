using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Hull Moving Average (HMA) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class HullMovingAverage
{
    /// <summary>
    /// Creates a Hull Moving Average indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Hull Moving Average parameters</param>
    /// <returns>A Hull Moving Average indicator instance</returns>
    public static IHullMovingAverage<TPrice, TOutput> Create<TPrice, TOutput>(PHullMovingAverage<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new HullMovingAverage_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new HullMovingAverage_FP<TPrice, TOutput>(parameters), // FP is already optimized with efficient WMA
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Hull Moving Average indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Hull Moving Average calculation (default: 14)</param>
    /// <returns>A Hull Moving Average indicator instance</returns>
    public static IHullMovingAverage<TPrice, TOutput> Create<TPrice, TOutput>(int period = 14)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PHullMovingAverage<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IHullMovingAverage<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PHullMovingAverage<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var hmaQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.HullMovingAverage_QC`2");
            
            if (hmaQcType != null)
            {
                // Make the generic type
                var genericType = hmaQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IHullMovingAverage<TPrice, TOutput> hma)
                {
                    return hma;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new HullMovingAverage_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IHullMovingAverage<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PHullMovingAverage<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - Hull Moving Average is computationally intensive with multiple WMAs
        // - First-party implementation uses optimized circular buffer WMAs for better performance
        // - For smaller periods (< 100), the first-party implementation is preferred for efficiency
        // - For larger periods or when QuantConnect is already loaded, consider QuantConnect
        // - Default to first-party for its optimized performance

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded && parameters.Period > 100)
        {
            // Use QuantConnect for very large periods when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for optimal performance
            return new HullMovingAverage_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a Hull Moving Average indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Hull Moving Average calculation</param>
    /// <returns>A Hull Moving Average indicator instance for double values</returns>
    public static IHullMovingAverage<double, double> CreateDouble(int period = 14)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create a Hull Moving Average indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Hull Moving Average calculation</param>
    /// <returns>A Hull Moving Average indicator instance for decimal values</returns>
    public static IHullMovingAverage<decimal, decimal> CreateDecimal(int period = 14)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a Hull Moving Average indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Hull Moving Average calculation</param>
    /// <returns>A Hull Moving Average indicator instance for float values</returns>
    public static IHullMovingAverage<float, float> CreateFloat(int period = 14)
    {
        return Create<float, float>(period);
    }
}
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Volume Weighted Moving Average (VWMA) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class VWMA
{
    /// <summary>
    /// Creates a VWMA indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The VWMA parameters</param>
    /// <returns>A VWMA indicator instance</returns>
    public static IVWMA<TInput, TOutput> Create<TInput, TOutput>(PVWMA<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new VWMA_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new VWMA_FP<TInput, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a VWMA indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the VWMA calculation (default: 20)</param>
    /// <returns>A VWMA indicator instance</returns>
    public static IVWMA<TInput, TOutput> Create<TInput, TOutput>(int period = 20)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PVWMA<TInput, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IVWMA<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PVWMA<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation from this assembly
            var currentAssembly = Assembly.GetExecutingAssembly();
            var vwmaqcType = currentAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.VWMA_QC`2");
            
            if (vwmaqcType != null)
            {
                // Make the generic type
                var genericType = vwmaqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IVWMA<TInput, TOutput> vwma)
                {
                    return vwma;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new VWMA_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IVWMA<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PVWMA<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - For small periods (< 50), the first-party implementation with circular buffer is very efficient
        // - For larger periods or when QuantConnect is already loaded, use QuantConnect (if available)
        // - Default to first-party for its simplicity and efficiency

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("QuantConnect") == true);

        if (quantConnectLoaded && parameters.Period > 50)
        {
            // Use QuantConnect for larger periods when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency
            return new VWMA_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a VWMA indicator for double values.
    /// </summary>
    /// <param name="period">The period for the VWMA calculation</param>
    /// <returns>A VWMA indicator instance for double values</returns>
    public static IVWMA<double, double> CreateDouble(int period = 20)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create a VWMA indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the VWMA calculation</param>
    /// <returns>A VWMA indicator instance for decimal values</returns>
    public static IVWMA<decimal, decimal> CreateDecimal(int period = 20)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a VWMA indicator for float values.
    /// </summary>
    /// <param name="period">The period for the VWMA calculation</param>
    /// <returns>A VWMA indicator instance for float values</returns>
    public static IVWMA<float, float> CreateFloat(int period = 20)
    {
        return Create<float, float>(period);
    }

    /// <summary>
    /// Convenience method to create a VWMA indicator for (price, volume) tuple inputs.
    /// </summary>
    /// <param name="period">The period for the VWMA calculation</param>
    /// <returns>A VWMA indicator instance for tuple inputs</returns>
    public static IVWMA<(double price, double volume), double> CreateForTuple(int period = 20)
    {
        return Create<(double, double), double>(period);
    }
}
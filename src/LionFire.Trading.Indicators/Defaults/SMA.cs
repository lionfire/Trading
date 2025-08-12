using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Simple Moving Average (SMA) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class SMA
{
    /// <summary>
    /// Creates an SMA indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The SMA parameters</param>
    /// <returns>An SMA indicator instance</returns>
    public static ISMA<TPrice, TOutput> Create<TPrice, TOutput>(PSMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new SMA_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new SMA_FP<TPrice, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an SMA indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the SMA calculation (default: 20)</param>
    /// <returns>An SMA indicator instance</returns>
    public static ISMA<TPrice, TOutput> Create<TPrice, TOutput>(int period = 20)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PSMA<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static ISMA<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PSMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var smaqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.SMA_QC`2");
            
            if (smaqcType != null)
            {
                // Make the generic type
                var genericType = smaqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is ISMA<TPrice, TOutput> sma)
                {
                    return sma;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new SMA_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static ISMA<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PSMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - For small periods (< 50), the first-party implementation with circular buffer is very efficient
        // - For larger periods or when QuantConnect is already loaded, use QuantConnect
        // - Default to first-party for its simplicity and efficiency

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded && parameters.Period > 50)
        {
            // Use QuantConnect for larger periods when it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency
            return new SMA_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an SMA indicator for double values.
    /// </summary>
    /// <param name="period">The period for the SMA calculation</param>
    /// <returns>An SMA indicator instance for double values</returns>
    public static ISMA<double, double> CreateDouble(int period = 20)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create an SMA indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the SMA calculation</param>
    /// <returns>An SMA indicator instance for decimal values</returns>
    public static ISMA<decimal, decimal> CreateDecimal(int period = 20)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create an SMA indicator for float values.
    /// </summary>
    /// <param name="period">The period for the SMA calculation</param>
    /// <returns>An SMA indicator instance for float values</returns>
    public static ISMA<float, float> CreateFloat(int period = 20)
    {
        return Create<float, float>(period);
    }
}
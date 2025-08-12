using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Exponential Moving Average (EMA) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class EMA
{
    /// <summary>
    /// Creates an EMA indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The EMA parameters</param>
    /// <returns>An EMA indicator instance</returns>
    public static IEMA<TPrice, TOutput> Create<TPrice, TOutput>(PEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new EMA_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an EMA indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="smoothingFactor">Optional smoothing factor override (default: calculated as 2/(period+1))</param>
    /// <returns>An EMA indicator instance</returns>
    public static IEMA<TPrice, TOutput> Create<TPrice, TOutput>(int period = 20, TOutput? smoothingFactor = null)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PEMA<TPrice, TOutput> 
        { 
            Period = period,
            SmoothingFactor = smoothingFactor
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IEMA<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var emaqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.EMA_QC`2");
            
            if (emaqcType != null)
            {
                // Make the generic type
                var genericType = emaqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IEMA<TPrice, TOutput> ema)
                {
                    return ema;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new EMA_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static IEMA<TPrice, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // For EMA, both implementations are fairly optimized
        // QuantConnect might have slightly better numerical stability for extreme values
        // First-party has the advantage of using SMA seed which can provide smoother initial values
        
        // Check if QuantConnect is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded)
        {
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            return new EMA_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IEMA<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - EMA is computationally simple, so both implementations perform well
        // - First-party uses SMA for initial seed, which can be beneficial for stability
        // - QuantConnect has been battle-tested in production environments
        // - Default to QuantConnect if available for maximum compatibility

        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Use QuantConnect if it's already loaded
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation for efficiency and SMA seeding
            return new EMA_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an EMA indicator for double values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>An EMA indicator instance for double values</returns>
    public static IEMA<double, double> CreateDouble(int period = 20, double? smoothingFactor = null)
    {
        return Create<double, double>(period, smoothingFactor);
    }

    /// <summary>
    /// Convenience method to create an EMA indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>An EMA indicator instance for decimal values</returns>
    public static IEMA<decimal, decimal> CreateDecimal(int period = 20, decimal? smoothingFactor = null)
    {
        return Create<decimal, decimal>(period, smoothingFactor);
    }

    /// <summary>
    /// Convenience method to create an EMA indicator for float values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>An EMA indicator instance for float values</returns>
    public static IEMA<float, float> CreateFloat(int period = 20, float? smoothingFactor = null)
    {
        return Create<float, float>(period, smoothingFactor);
    }
}
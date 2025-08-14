using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default ADX (Average Directional Index) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ADX
{
    /// <summary>
    /// Creates an ADX indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The ADX parameters</param>
    /// <returns>An ADX indicator instance</returns>
    public static IADX<TInput, TOutput> Create<TInput, TOutput>(PADX<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new ADX_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new ADX_FP<TInput, TOutput>(parameters), // FP is already optimized with circular buffers
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an ADX indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the ADX calculation (default: 14)</param>
    /// <returns>An ADX indicator instance</returns>
    public static IADX<TInput, TOutput> Create<TInput, TOutput>(int period = 14)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PADX<TInput, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IADX<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PADX<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect ADX implementation for period: {Period}", parameters.Period);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var adxqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ADX_QC`2");
            
            if (adxqcType != null)
            {
                // Make the generic type
                var genericType = adxqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IADX<TInput, TOutput> adx)
                {
                    logger?.LogDebug("Successfully created QuantConnect ADX implementation");
                    return adx;
                }
            }
            
            logger?.LogWarning("QuantConnect ADX type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect ADX implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party ADX implementation");
        return new ADX_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IADX<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PADX<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - ADX requires True Range calculation and complex smoothing (EMA of smoothed positive/negative directional indicators)
        // - First-party uses circular buffers for efficient TR, +DI, -DI calculations
        // - QuantConnect has battle-tested numerical precision for edge cases
        // - Memory usage scales linearly with period size due to multiple smoothing buffers
        // - Performance critical for short periods due to frequent calculations
        
        logger?.LogDebug("Selecting best ADX implementation for period: {Period}", parameters.Period);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isShortPeriod = parameters.Period < 30;
        bool isVeryLargePeriod = parameters.Period > 100;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("ADX selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Short period={ShortPeriod}, Very large period={VeryLargePeriod}", 
            quantConnectLoaded, isMemoryConstrained, isShortPeriod, isVeryLargePeriod);

        if (quantConnectLoaded && isVeryLargePeriod && !isMemoryConstrained)
        {
            // Use QuantConnect for very large periods when it's already loaded and memory isn't constrained
            logger?.LogDebug("Selected QuantConnect ADX for very large period scenario");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (isMemoryConstrained && isShortPeriod)
        {
            // Use first-party for memory-constrained short period scenarios (most efficient)
            logger?.LogDebug("Selected First-Party ADX for memory-constrained short period scenario");
            return new ADX_FP<TInput, TOutput>(parameters);
        }
        else if (quantConnectLoaded && parameters.Period > 50)
        {
            // Use QuantConnect for larger periods when it's already loaded
            logger?.LogDebug("Selected QuantConnect ADX for large period with QC already loaded");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Default to first-party for its efficiency and precise ADX calculation with circular buffers
            logger?.LogDebug("Selected First-Party ADX as default choice for optimal performance");
            return new ADX_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an ADX indicator for double values.
    /// </summary>
    /// <param name="period">The period for the ADX calculation</param>
    /// <returns>An ADX indicator instance for double values</returns>
    public static IADX<double, double> CreateDouble(int period = 14)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create an ADX indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the ADX calculation</param>
    /// <returns>An ADX indicator instance for decimal values</returns>
    public static IADX<decimal, decimal> CreateDecimal(int period = 14)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create an ADX indicator for float values.
    /// </summary>
    /// <param name="period">The period for the ADX calculation</param>
    /// <returns>An ADX indicator instance for float values</returns>
    public static IADX<float, float> CreateFloat(int period = 14)
    {
        return Create<float, float>(period);
    }
    
    /// <summary>
    /// Gets a logger instance for diagnostic purposes.
    /// </summary>
    private static ILogger? GetLogger()
    {
        try
        {
            // Try to get logger from a service provider if available
            // This is a best-effort attempt and won't break if logging isn't configured
            var loggerFactory = ServiceCollectionContainerBuilderExtensions
                .BuildServiceProvider(new ServiceCollection())
                .GetService<ILoggerFactory>();
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.ADX");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
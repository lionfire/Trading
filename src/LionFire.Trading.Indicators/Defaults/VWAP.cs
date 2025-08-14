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
/// Default Volume Weighted Average Price (VWAP) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class VWAP
{
    /// <summary>
    /// Creates a VWAP indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The VWAP parameters</param>
    /// <returns>A VWAP indicator instance</returns>
    public static IVWAP<TInput, TOutput> Create<TInput, TOutput>(PVWAP<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new VWAP_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new VWAP_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a VWAP indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="resetPeriod">The reset period for VWAP calculation (default: Daily)</param>
    /// <param name="useTypicalPrice">Whether to use typical price (H+L+C)/3 instead of close price (default: true)</param>
    /// <returns>A VWAP indicator instance</returns>
    public static IVWAP<TInput, TOutput> Create<TInput, TOutput>(
        VWAPResetPeriod resetPeriod = VWAPResetPeriod.Daily, 
        bool useTypicalPrice = true)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PVWAP<TInput, TOutput> 
        { 
            ResetPeriod = resetPeriod,
            UseTypicalPrice = useTypicalPrice
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a VWAP indicator with custom reset time.
    /// </summary>
    /// <typeparam name="TInput">The input data type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="customResetTime">The custom reset time (requires resetPeriod = Custom)</param>
    /// <param name="useTypicalPrice">Whether to use typical price (H+L+C)/3 instead of close price (default: true)</param>
    /// <returns>A VWAP indicator instance</returns>
    public static IVWAP<TInput, TOutput> CreateWithCustomReset<TInput, TOutput>(
        TimeSpan customResetTime,
        bool useTypicalPrice = true)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PVWAP<TInput, TOutput> 
        { 
            ResetPeriod = VWAPResetPeriod.Custom,
            CustomResetTime = customResetTime,
            UseTypicalPrice = useTypicalPrice
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IVWAP<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PVWAP<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect VWAP implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var vwapqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.VWAP_QC`2");
            
            if (vwapqcType != null)
            {
                // Make the generic type
                var genericType = vwapqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IVWAP<TInput, TOutput> vwap)
                {
                    logger?.LogDebug("Successfully created QuantConnect VWAP implementation");
                    return vwap;
                }
            }
            
            logger?.LogWarning("QuantConnect VWAP type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect VWAP implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party VWAP implementation");
        return new VWAP_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IVWAP<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PVWAP<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - VWAP requires volume-weighted price accumulation with frequent resets
        // - First-party has optimized reset period handling and memory management
        // - QuantConnect is good for compatibility but has more overhead for reset operations
        // - Memory usage depends on reset frequency and data accumulation
        // - Performance considerations for high-frequency data streams
        
        logger?.LogDebug("Selecting best VWAP implementation for reset period: {ResetPeriod}", parameters.ResetPeriod);

        // Memory and performance analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isHighFrequencyScenario = parameters.ResetPeriod == VWAPResetPeriod.Never;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific optimizations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("VWAP selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, High freq={HighFrequency}", 
            quantConnectLoaded, isMemoryConstrained, isHighFrequencyScenario);

        if (quantConnectLoaded && 
            isHighFrequencyScenario && 
            !isMemoryConstrained &&
            parameters.ResetPeriod == VWAPResetPeriod.Never)
        {
            // Use QuantConnect for simple never-reset scenarios when it's already loaded
            logger?.LogDebug("Selected QuantConnect VWAP for never-reset high-frequency scenario");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (parameters.ResetPeriod != VWAPResetPeriod.Never || isMemoryConstrained)
        {
            // Use first-party for complex reset scenarios or memory-constrained environments
            logger?.LogDebug("Selected First-Party VWAP for advanced reset handling or memory optimization");
            return new VWAP_FP<TInput, TOutput>(parameters);
        }
        else
        {
            // Default to first-party for its superior features and efficiency
            logger?.LogDebug("Selected First-Party VWAP as default choice");
            return new VWAP_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a daily VWAP indicator for double values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for double values</returns>
    public static IVWAP<double, double> CreateDailyDouble()
    {
        return Create<double, double>(VWAPResetPeriod.Daily, true);
    }

    /// <summary>
    /// Convenience method to create a daily VWAP indicator for decimal values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for decimal values</returns>
    public static IVWAP<decimal, decimal> CreateDailyDecimal()
    {
        return Create<decimal, decimal>(VWAPResetPeriod.Daily, true);
    }

    /// <summary>
    /// Convenience method to create a daily VWAP indicator for float values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for float values</returns>
    public static IVWAP<float, float> CreateDailyFloat()
    {
        return Create<float, float>(VWAPResetPeriod.Daily, true);
    }

    /// <summary>
    /// Convenience method to create a never-reset VWAP indicator for double values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for double values</returns>
    public static IVWAP<double, double> CreateRunningDouble()
    {
        return Create<double, double>(VWAPResetPeriod.Never, true);
    }

    /// <summary>
    /// Convenience method to create a never-reset VWAP indicator for decimal values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for decimal values</returns>
    public static IVWAP<decimal, decimal> CreateRunningDecimal()
    {
        return Create<decimal, decimal>(VWAPResetPeriod.Never, true);
    }

    /// <summary>
    /// Convenience method to create a never-reset VWAP indicator for float values with typical price.
    /// </summary>
    /// <returns>A VWAP indicator instance for float values</returns>
    public static IVWAP<float, float> CreateRunningFloat()
    {
        return Create<float, float>(VWAPResetPeriod.Never, true);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.VWAP");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
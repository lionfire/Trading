using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Donchian Channels indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class DonchianChannels
{
    /// <summary>
    /// Creates a Donchian Channels indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The price data type (should have High, Low, and Close properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Donchian Channels parameters</param>
    /// <returns>A Donchian Channels indicator instance</returns>
    public static IDonchianChannels<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PDonchianChannels<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new DonchianChannels_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new DonchianChannels_FP<TPrice, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Donchian Channels indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The price data type (should have High, Low, and Close properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <returns>A Donchian Channels indicator instance</returns>
    public static IDonchianChannels<HLC<TPrice>, TOutput> Create<TPrice, TOutput>()
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PDonchianChannels<TPrice, TOutput>();
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IDonchianChannels<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PDonchianChannels<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Donchian Channels implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var donchianQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.DonchianChannels_QC`2");
            
            if (donchianQcType != null)
            {
                // Make the generic type
                var genericType = donchianQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IDonchianChannels<HLC<TPrice>, TOutput> donchian)
                {
                    logger?.LogDebug("Successfully created QuantConnect Donchian Channels implementation");
                    return donchian;
                }
            }
            
            logger?.LogWarning("QuantConnect Donchian Channels type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Donchian Channels implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Donchian Channels implementation");
        return new DonchianChannels_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IDonchianChannels<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PDonchianChannels<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - Donchian Channels requires tracking min/max over a period (similar to Williams %R)
        // - First-party uses optimized circular buffers for efficient min/max tracking
        // - QuantConnect may have better numerical precision for extreme values
        // - Consider memory usage based on period size
        
        // Gather runtime information for decision making
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        var availableMemoryMB = GetAvailableMemoryMB();
        var period = parameters.Period;
        
        logger?.LogDebug("Donchian Channels auto-selection: Period={Period}, QC loaded={QCLoaded}, Memory={MemoryMB}MB", 
            period, quantConnectLoaded, availableMemoryMB);

        // For very short periods, always prefer first-party due to minimal overhead
        if (period < 10)
        {
            logger?.LogInformation("Selected First-Party Donchian Channels (very short period: {Period})", period);
            return new DonchianChannels_FP<TPrice, TOutput>(parameters);
        }
        
        // Performance-based selection logic for longer periods
        if (quantConnectLoaded)
        {
            // If QC is loaded, decide based on period size and memory
            if (period > 50 && availableMemoryMB > 256)
            {
                // For larger periods with adequate memory, QC might have better optimizations
                logger?.LogInformation("Selected QuantConnect Donchian Channels (large period with sufficient memory)");
                var qcImplementation = CreateQuantConnectImplementation(parameters);
                if (qcImplementation is DonchianChannels_FP<TPrice, TOutput>)
                {
                    logger?.LogWarning("QuantConnect Donchian Channels creation failed, using First-Party fallback");
                }
                return qcImplementation;
            }
            else if (period <= 25 || availableMemoryMB < 128)
            {
                // For medium periods or limited memory, prefer FP's circular buffer efficiency
                logger?.LogInformation("Selected First-Party Donchian Channels (medium period or limited memory)");
                return new DonchianChannels_FP<TPrice, TOutput>(parameters);
            }
            else
            {
                // Medium-large periods - use QC for consistency since it's loaded
                logger?.LogInformation("Selected QuantConnect Donchian Channels (medium-large period, QC already loaded)");
                return CreateQuantConnectImplementation(parameters);
            }
        }
        else
        {
            // QC not loaded - prefer FP unless period is extremely large
            if (period > 100 && availableMemoryMB > 512)
            {
                logger?.LogInformation("Loading QuantConnect Donchian Channels for very large period (Period={Period})", period);
                return CreateQuantConnectImplementation(parameters);
            }
            else
            {
                logger?.LogInformation("Selected First-Party Donchian Channels (QC not loaded, efficient for period {Period})", period);
                return new DonchianChannels_FP<TPrice, TOutput>(parameters);
            }
        }
    }

    /// <summary>
    /// Convenience method to create a Donchian Channels indicator for Bar input with double output.
    /// </summary>
    /// <param name="period">The period for calculation (default: 20)</param>
    /// <returns>A Donchian Channels indicator instance for Bar input and double output</returns>
    public static IDonchianChannels<HLC<double>, double> CreateBarDouble(int period = 20)
    {
        var parameters = new PDonchianChannels<double, double> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Convenience method to create a Donchian Channels indicator for TimedBar input with double output.
    /// </summary>
    /// <param name="period">The period for calculation (default: 20)</param>
    /// <returns>A Donchian Channels indicator instance for TimedBar input and double output</returns>
    public static IDonchianChannels<HLC<double>, double> CreateTimedBarDouble(int period = 20)
    {
        var parameters = new PDonchianChannels<double, double> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Convenience method to create a Donchian Channels indicator for Bar input with decimal output.
    /// </summary>
    /// <param name="period">The period for calculation (default: 20)</param>
    /// <returns>A Donchian Channels indicator instance for Bar input and decimal output</returns>
    public static IDonchianChannels<HLC<decimal>, decimal> CreateBarDecimal(int period = 20)
    {
        var parameters = new PDonchianChannels<decimal, decimal> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Convenience method to create a Donchian Channels indicator for TimedBar input with decimal output.
    /// </summary>
    /// <param name="period">The period for calculation (default: 20)</param>
    /// <returns>A Donchian Channels indicator instance for TimedBar input and decimal output</returns>
    public static IDonchianChannels<HLC<decimal>, decimal> CreateTimedBarDecimal(int period = 20)
    {
        var parameters = new PDonchianChannels<decimal, decimal> { Period = period };
        return Create(parameters);
    }
    
    #region Private Helper Methods
    
    /// <summary>
    /// Gets a logger instance for implementation selection decisions.
    /// </summary>
    private static ILogger? GetLogger()
    {
        try
        {
            // Try to get logger from a service provider if available
            // This is a best-effort attempt and won't break if logging isn't configured
            var loggerFactory = Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions
                .BuildServiceProvider(new Microsoft.Extensions.DependencyInjection.ServiceCollection())
                .GetService<ILoggerFactory>();
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.DonchianChannels");
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Checks if QuantConnect assembly is already loaded to avoid unnecessary assembly loading.
    /// </summary>
    private static bool IsQuantConnectAssemblyLoaded()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
    }
    
    /// <summary>
    /// Gets available memory in megabytes for implementation selection decisions.
    /// </summary>
    private static long GetAvailableMemoryMB()
    {
        try
        {
            // Get working set memory as a proxy for available memory
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSetMB = process.WorkingSet64 / (1024 * 1024);
            
            // Donchian Channels memory usage scales with period size for min/max tracking
            return Math.Max(128, 1536 - workingSetMB); // Moderate memory requirements
        }
        catch
        {
            return 256; // Conservative default for Donchian Channels
        }
    }
    
    #endregion
}
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default On Balance Volume (OBV) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class OBV
{
    /// <summary>
    /// Creates an OBV indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have Close and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The OBV parameters</param>
    /// <returns>An OBV indicator instance</returns>
    public static IOBV<TInput, TOutput> Create<TInput, TOutput>(POBV<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new OBV_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new OBV_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an OBV indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input data type (should have Close and Volume properties)</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <returns>An OBV indicator instance</returns>
    public static IOBV<TInput, TOutput> Create<TInput, TOutput>()
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new POBV<TInput, TOutput>();
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IOBV<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(POBV<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect OBV implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var obvQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.OBV_QC`2");
            
            if (obvQcType != null)
            {
                // Make the generic type
                var genericType = obvQcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IOBV<TInput, TOutput> obv)
                {
                    logger?.LogDebug("Successfully created QuantConnect OBV implementation");
                    return obv;
                }
            }
            
            logger?.LogWarning("QuantConnect OBV type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect OBV implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party OBV implementation");
        return new OBV_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IOBV<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(POBV<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - OBV is computationally simple (just cumulative volume based on price direction)
        // - First-party implementation is lightweight and efficient for streaming data
        // - QuantConnect provides battle-tested numerical stability
        // - Memory usage is minimal since OBV only maintains cumulative state
        
        // Gather runtime information for decision making
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        var availableMemoryMB = GetAvailableMemoryMB();
        
        logger?.LogDebug("OBV auto-selection: QC loaded={QCLoaded}, Memory={MemoryMB}MB", 
            quantConnectLoaded, availableMemoryMB);

        // Performance-based selection logic
        if (quantConnectLoaded)
        {
            // OBV is simple enough that both implementations perform similarly
            // If QC is loaded, use it for consistency with other indicators
            if (availableMemoryMB > 128)
            {
                logger?.LogInformation("Selected QuantConnect OBV (QC loaded with sufficient memory)");
                var qcImplementation = CreateQuantConnectImplementation(parameters);
                if (qcImplementation is OBV_FP<TInput, TOutput>)
                {
                    logger?.LogWarning("QuantConnect OBV creation failed, using First-Party fallback");
                }
                return qcImplementation;
            }
            else
            {
                // Very limited memory - use lightweight FP implementation
                logger?.LogInformation("Selected First-Party OBV (limited memory)");
                return new OBV_FP<TInput, TOutput>(parameters);
            }
        }
        else
        {
            // QC not loaded - prefer FP for its simplicity and lightweight nature
            logger?.LogInformation("Selected First-Party OBV (QC not loaded, lightweight implementation preferred)");
            return new OBV_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an OBV indicator for TimedBarStruct input with double output.
    /// </summary>
    /// <returns>An OBV indicator instance for TimedBarStruct input and double output</returns>
    public static IOBV<TimedBarStruct, double> CreateBarDouble()
    {
        return Create<TimedBarStruct, double>();
    }

    /// <summary>
    /// Convenience method to create an OBV indicator for TimedBar input with double output.
    /// </summary>
    /// <returns>An OBV indicator instance for TimedBar input and double output</returns>
    public static IOBV<TimedBar, double> CreateTimedBarDouble()
    {
        return Create<TimedBar, double>();
    }

    /// <summary>
    /// Convenience method to create an OBV indicator for TimedBarStruct input with decimal output.
    /// </summary>
    /// <returns>An OBV indicator instance for TimedBarStruct input and decimal output</returns>
    public static IOBV<TimedBarStruct, decimal> CreateBarDecimal()
    {
        return Create<TimedBarStruct, decimal>();
    }

    /// <summary>
    /// Convenience method to create an OBV indicator for TimedBar input with decimal output.
    /// </summary>
    /// <returns>An OBV indicator instance for TimedBar input and decimal output</returns>
    public static IOBV<TimedBar, decimal> CreateTimedBarDecimal()
    {
        return Create<TimedBar, decimal>();
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.OBV");
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
            
            // OBV has minimal memory requirements, so be more conservative
            return Math.Max(128, 1024 - workingSetMB);
        }
        catch
        {
            return 256; // Very conservative default for OBV
        }
    }
    
    #endregion
}
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default CCI (Commodity Channel Index) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class CCI
{
    /// <summary>
    /// Creates a CCI indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The CCI parameters</param>
    /// <returns>A CCI indicator instance</returns>
    public static ICCI<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PCCI<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new CCI_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new CCI_FP<TPrice, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a CCI indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the CCI calculation (default: 20)</param>
    /// <param name="constant">The constant for CCI calculation (default: 0.015)</param>
    /// <returns>A CCI indicator instance</returns>
    public static ICCI<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(int period = 20, double constant = 0.015)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PCCI<TPrice, TOutput> 
        { 
            Period = period,
            Constant = constant
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static ICCI<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PCCI<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect CCI implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var cciqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.CCI_QC`2");
            
            if (cciqcType != null)
            {
                // Make the generic type
                var genericType = cciqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is ICCI<HLC<TPrice>, TOutput> cci)
                {
                    logger?.LogDebug("Successfully created QuantConnect CCI implementation");
                    return cci;
                }
            }
            
            logger?.LogWarning("QuantConnect CCI type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect CCI implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party CCI implementation");
        return new CCI_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static ICCI<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PCCI<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - CCI requires SMA calculation and mean absolute deviation computation
        // - First-party uses circular buffer for efficiency in streaming scenarios
        // - QuantConnect has battle-tested numerical stability for extreme values
        // - Custom constants may require specialized handling
        // - Memory usage scales with period size
        
        // Gather runtime information for decision making
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        var availableMemoryMB = GetAvailableMemoryMB();
        var period = parameters.Period;
        var isCustomConstant = Math.Abs(parameters.Constant - 0.015) > 0.001;
        
        logger?.LogDebug("CCI auto-selection: Period={Period}, CustomConstant={CustomConstant}, QC loaded={QCLoaded}, Memory={MemoryMB}MB", 
            period, isCustomConstant, quantConnectLoaded, availableMemoryMB);

        // Always prefer first-party for custom constants (better control over calculation)
        if (isCustomConstant)
        {
            logger?.LogInformation("Selected First-Party CCI (custom constant: {Constant})", parameters.Constant);
            return new CCI_FP<TPrice, TOutput>(parameters);
        }
        
        // Performance-based selection logic for standard constant
        if (quantConnectLoaded)
        {
            // If QC is loaded, decide based on period size and memory
            if (period > 75 && availableMemoryMB > 512)
            {
                // For large periods with adequate memory, QC might have better numerical precision
                logger?.LogInformation("Selected QuantConnect CCI (large period with sufficient memory)");
                var qcImplementation = CreateQuantConnectImplementation(parameters);
                if (qcImplementation is CCI_FP<TPrice, TOutput>)
                {
                    logger?.LogWarning("QuantConnect CCI creation failed, using First-Party fallback");
                }
                return qcImplementation;
            }
            else if (period <= 30 || availableMemoryMB < 256)
            {
                // For small periods or limited memory, prefer FP's circular buffer efficiency
                logger?.LogInformation("Selected First-Party CCI (small period or limited memory)");
                return new CCI_FP<TPrice, TOutput>(parameters);
            }
            else
            {
                // Medium periods - use QC for consistency since it's loaded
                logger?.LogInformation("Selected QuantConnect CCI (medium period, QC already loaded)");
                return CreateQuantConnectImplementation(parameters);
            }
        }
        else
        {
            // QC not loaded - prefer FP unless period is very large
            if (period > 150 && availableMemoryMB > 1024)
            {
                logger?.LogInformation("Loading QuantConnect CCI for very large period (Period={Period})", period);
                return CreateQuantConnectImplementation(parameters);
            }
            else
            {
                logger?.LogInformation("Selected First-Party CCI (QC not loaded, efficient for period {Period})", period);
                return new CCI_FP<TPrice, TOutput>(parameters);
            }
        }
    }

    /// <summary>
    /// Convenience method to create a CCI indicator for double values.
    /// </summary>
    /// <param name="period">The period for the CCI calculation</param>
    /// <param name="constant">The constant for CCI calculation</param>
    /// <returns>A CCI indicator instance for double values</returns>
    public static ICCI<HLC<double>, double> CreateDouble(int period = 20, double constant = 0.015)
    {
        return Create<double, double>(period, constant);
    }

    /// <summary>
    /// Convenience method to create a CCI indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the CCI calculation</param>
    /// <param name="constant">The constant for CCI calculation</param>
    /// <returns>A CCI indicator instance for decimal values</returns>
    public static ICCI<HLC<decimal>, decimal> CreateDecimal(int period = 20, double constant = 0.015)
    {
        return Create<decimal, decimal>(period, constant);
    }

    /// <summary>
    /// Convenience method to create a CCI indicator for float values.
    /// </summary>
    /// <param name="period">The period for the CCI calculation</param>
    /// <param name="constant">The constant for CCI calculation</param>
    /// <returns>A CCI indicator instance for float values</returns>
    public static ICCI<HLC<float>, float> CreateFloat(int period = 20, double constant = 0.015)
    {
        return Create<float, float>(period, constant);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.CCI");
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
            
            // CCI requires SMA and mean deviation calculation, moderate memory usage
            return Math.Max(256, 2048 - workingSetMB); // Reasonable memory estimate
        }
        catch
        {
            return 512; // Conservative default for CCI
        }
    }
    
    #endregion
}
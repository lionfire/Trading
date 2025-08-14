using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Rate of Change (ROC) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ROC
{
    /// <summary>
    /// Creates a ROC indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The ROC parameters</param>
    /// <returns>A ROC indicator instance</returns>
    public static IROC<TPrice, TOutput> Create<TPrice, TOutput>(PROC<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new ROC_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new ROC_FP<TPrice, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a ROC indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the ROC calculation (default: 10)</param>
    /// <returns>A ROC indicator instance</returns>
    public static IROC<TPrice, TOutput> Create<TPrice, TOutput>(int period = 10)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PROC<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IROC<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PROC<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect ROC implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var rocqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ROC_QC`2");
            
            if (rocqcType != null)
            {
                // Make the generic type
                var genericType = rocqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IROC<TPrice, TOutput> roc)
                {
                    logger?.LogDebug("Successfully created QuantConnect ROC implementation");
                    return roc;
                }
            }
            
            logger?.LogWarning("QuantConnect ROC type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect ROC implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party ROC implementation");
        return new ROC_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IROC<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PROC<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - ROC is computationally simple but needs historical data storage
        // - First-party implementation uses circular buffer for efficiency
        // - QuantConnect has battle-tested algorithms for numerical stability
        // - Consider memory usage and computational requirements
        
        // Gather runtime information for decision making
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        var availableMemoryMB = GetAvailableMemoryMB();
        var period = parameters.Period;
        
        logger?.LogDebug("ROC auto-selection: Period={Period}, QC loaded={QCLoaded}, Memory={MemoryMB}MB", 
            period, quantConnectLoaded, availableMemoryMB);

        // Performance-based selection logic
        if (quantConnectLoaded)
        {
            // If QC is loaded, decide based on period size and memory
            if (period > 100 && availableMemoryMB > 512)
            {
                // For large periods with adequate memory, QC might be more optimized
                logger?.LogInformation("Selected QuantConnect ROC (large period with sufficient memory)");
                return CreateQuantConnectImplementation(parameters);
            }
            else if (period <= 30 || availableMemoryMB < 128)
            {
                // For small periods or very limited memory, prefer FP's efficiency
                logger?.LogInformation("Selected First-Party ROC (small period or limited memory)");
                return new ROC_FP<TPrice, TOutput>(parameters);
            }
            else
            {
                // Medium periods - use QC for consistency since it's loaded
                logger?.LogInformation("Selected QuantConnect ROC (medium period, QC already loaded)");
                var qcImplementation = CreateQuantConnectImplementation(parameters);
                if (qcImplementation is ROC_FP<TPrice, TOutput>)
                {
                    logger?.LogWarning("QuantConnect ROC creation failed, using First-Party fallback");
                }
                return qcImplementation;
            }
        }
        else
        {
            // QC not loaded - use FP unless period is extremely large
            if (period > 250 && availableMemoryMB > 1024)
            {
                logger?.LogInformation("Loading QuantConnect ROC for very large period (Period={Period})", period);
                return CreateQuantConnectImplementation(parameters);
            }
            else
            {
                logger?.LogInformation("Selected First-Party ROC (QC not loaded, efficient for period {Period})", period);
                return new ROC_FP<TPrice, TOutput>(parameters);
            }
        }
    }

    /// <summary>
    /// Convenience method to create a ROC indicator for double values.
    /// </summary>
    /// <param name="period">The period for the ROC calculation</param>
    /// <returns>A ROC indicator instance for double values</returns>
    public static IROC<double, double> CreateDouble(int period = 10)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create a ROC indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the ROC calculation</param>
    /// <returns>A ROC indicator instance for decimal values</returns>
    public static IROC<decimal, decimal> CreateDecimal(int period = 10)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a ROC indicator for float values.
    /// </summary>
    /// <param name="period">The period for the ROC calculation</param>
    /// <returns>A ROC indicator instance for float values</returns>
    public static IROC<float, float> CreateFloat(int period = 10)
    {
        return Create<float, float>(period);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.ROC");
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
            
            // Estimate available memory based on system constraints
            // This is a heuristic - in production, more sophisticated memory management might be used
            return Math.Max(256, 2048 - workingSetMB); // More conservative for ROC
        }
        catch
        {
            return 512; // Default conservative estimate
        }
    }
    
    #endregion
}
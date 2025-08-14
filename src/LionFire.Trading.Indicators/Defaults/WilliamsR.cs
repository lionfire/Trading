using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Williams %R indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class WilliamsR
{
    /// <summary>
    /// Creates a Williams %R indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Williams %R parameters</param>
    /// <returns>A Williams %R indicator instance</returns>
    public static IWilliamsR<TPrice, TOutput> Create<TPrice, TOutput>(PWilliamsR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new WilliamsR_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Williams %R indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Williams %R calculation (default: 14)</param>
    /// <param name="overboughtLevel">The overbought threshold (default: -20)</param>
    /// <param name="oversoldLevel">The oversold threshold (default: -80)</param>
    /// <returns>A Williams %R indicator instance</returns>
    public static IWilliamsR<TPrice, TOutput> Create<TPrice, TOutput>(
        int period = 14, 
        TOutput? overboughtLevel = null, 
        TOutput? oversoldLevel = null)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PWilliamsR<TPrice, TOutput> 
        { 
            Period = period,
            OverboughtLevel = overboughtLevel ?? TOutput.CreateChecked(-20),
            OversoldLevel = oversoldLevel ?? TOutput.CreateChecked(-80)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IWilliamsR<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PWilliamsR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Williams %R implementation");
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var williamsRQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.WilliamsR_QC`2");
            
            if (williamsRQcType != null)
            {
                // Make the generic type
                var genericType = williamsRQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IWilliamsR<TPrice, TOutput> williamsR)
                {
                    logger?.LogDebug("Successfully created QuantConnect Williams %R implementation");
                    return williamsR;
                }
            }
            
            logger?.LogWarning("QuantConnect Williams %R type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Williams %R implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Williams %R implementation");
        return new WilliamsR_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static IWilliamsR<TPrice, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PWilliamsR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // For Williams %R, the first-party implementation with circular buffers
        // is likely more optimized for streaming scenarios due to efficient min/max tracking
        // QuantConnect implementation might have slightly better numerical stability for extreme values
        
        // Check available memory for decision making
        var availableMemoryMB = GetAvailableMemoryMB();
        logger?.LogDebug("Williams %R optimization selection: Available memory: {MemoryMB}MB, Period: {Period}", 
            availableMemoryMB, parameters.Period);
        
        // For periods <= 50, first-party circular buffer implementation is very efficient
        if (parameters.Period <= 50)
        {
            logger?.LogDebug("Selected First-Party Williams %R for optimization (small period: {Period})", parameters.Period);
            return new WilliamsR_FP<TPrice, TOutput>(parameters);
        }
        
        // For longer periods, check if QuantConnect is already loaded and memory availability
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        
        if (quantConnectLoaded && availableMemoryMB > 512)
        {
            logger?.LogDebug("Selected QuantConnect Williams %R for optimization (large period: {Period}, memory available: {MemoryMB}MB)", 
                parameters.Period, availableMemoryMB);
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            logger?.LogDebug("Selected First-Party Williams %R for optimization (fallback - QC loaded: {QCLoaded}, memory: {MemoryMB}MB)", 
                quantConnectLoaded, availableMemoryMB);
            return new WilliamsR_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IWilliamsR<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PWilliamsR<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - Williams %R requires tracking min/max over a period
        // - First-party uses circular buffers which are very efficient for this
        // - QuantConnect has been battle-tested in production environments
        // - Consider memory availability and computational complexity
        // - Default to first-party for streaming efficiency unless QC is already loaded

        // Gather runtime information for decision making
        bool quantConnectLoaded = IsQuantConnectAssemblyLoaded();
        var availableMemoryMB = GetAvailableMemoryMB();
        var period = parameters.Period;
        
        logger?.LogDebug("Williams %R auto-selection: Period={Period}, QC loaded={QCLoaded}, Memory={MemoryMB}MB", 
            period, quantConnectLoaded, availableMemoryMB);

        // Performance-based selection logic
        if (quantConnectLoaded)
        {
            // If QC is loaded, decide based on period size and memory
            if (period > 100 && availableMemoryMB > 1024)
            {
                // For very large periods with plenty of memory, QC might be more optimized
                logger?.LogInformation("Selected QuantConnect Williams %R (large period with sufficient memory)");
                return CreateQuantConnectImplementation(parameters);
            }
            else if (period <= 20 || availableMemoryMB < 256)
            {
                // For small periods or low memory, prefer FP's circular buffer efficiency
                logger?.LogInformation("Selected First-Party Williams %R (small period or limited memory)");
                return new WilliamsR_FP<TPrice, TOutput>(parameters);
            }
            else
            {
                // Medium periods with reasonable memory - try QC for compatibility, fallback to FP
                logger?.LogInformation("Selected QuantConnect Williams %R (medium period, QC already loaded)");
                var qcImplementation = CreateQuantConnectImplementation(parameters);
                if (qcImplementation is WilliamsR_FP<TPrice, TOutput>)
                {
                    logger?.LogWarning("QuantConnect Williams %R creation failed, using First-Party fallback");
                }
                return qcImplementation;
            }
        }
        else
        {
            // QC not loaded - prefer FP unless period is extremely large
            if (period > 200 && availableMemoryMB > 2048)
            {
                logger?.LogInformation("Loading QuantConnect Williams %R for very large period (Period={Period})", period);
                return CreateQuantConnectImplementation(parameters);
            }
            else
            {
                logger?.LogInformation("Selected First-Party Williams %R (QC not loaded, efficient for period {Period})", period);
                return new WilliamsR_FP<TPrice, TOutput>(parameters);
            }
        }
    }

    /// <summary>
    /// Convenience method to create a Williams %R indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Williams %R calculation</param>
    /// <param name="overboughtLevel">The overbought threshold</param>
    /// <param name="oversoldLevel">The oversold threshold</param>
    /// <returns>A Williams %R indicator instance for double values</returns>
    public static IWilliamsR<double, double> CreateDouble(int period = 14, double overboughtLevel = -20.0, double oversoldLevel = -80.0)
    {
        return Create<double, double>(period, overboughtLevel, oversoldLevel);
    }

    /// <summary>
    /// Convenience method to create a Williams %R indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Williams %R calculation</param>
    /// <param name="overboughtLevel">The overbought threshold</param>
    /// <param name="oversoldLevel">The oversold threshold</param>
    /// <returns>A Williams %R indicator instance for decimal values</returns>
    public static IWilliamsR<decimal, decimal> CreateDecimal(int period = 14, decimal overboughtLevel = -20.0m, decimal oversoldLevel = -80.0m)
    {
        return Create<decimal, decimal>(period, overboughtLevel, oversoldLevel);
    }

    /// <summary>
    /// Convenience method to create a Williams %R indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Williams %R calculation</param>
    /// <param name="overboughtLevel">The overbought threshold</param>
    /// <param name="oversoldLevel">The oversold threshold</param>
    /// <returns>A Williams %R indicator instance for float values</returns>
    public static IWilliamsR<float, float> CreateFloat(int period = 14, float overboughtLevel = -20.0f, float oversoldLevel = -80.0f)
    {
        return Create<float, float>(period, overboughtLevel, oversoldLevel);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.WilliamsR");
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
            return Math.Max(512, 4096 - workingSetMB); // Assume max 4GB, subtract current usage
        }
        catch
        {
            return 1024; // Default conservative estimate
        }
    }
    
    #endregion
}
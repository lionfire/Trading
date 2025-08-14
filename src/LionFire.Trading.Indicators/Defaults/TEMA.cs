using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Triple Exponential Moving Average (TEMA) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class TEMA
{
    /// <summary>
    /// Creates a TEMA indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The TEMA parameters</param>
    /// <returns>A TEMA indicator instance</returns>
    public static ITEMA<TPrice, TOutput> Create<TPrice, TOutput>(PTEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new TEMA_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a TEMA indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the TEMA calculation (default: 14)</param>
    /// <param name="smoothingFactor">Optional smoothing factor override (default: calculated as 2/(period+1))</param>
    /// <returns>A TEMA indicator instance</returns>
    public static ITEMA<TPrice, TOutput> Create<TPrice, TOutput>(int period = 14, TOutput? smoothingFactor = null)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PTEMA<TPrice, TOutput> 
        { 
            Period = period,
            SmoothingFactor = smoothingFactor
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static ITEMA<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PTEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect TEMA implementation for period {Period}", parameters.Period);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var temaqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.TEMA_QC`2");
            
            if (temaqcType != null)
            {
                // Make the generic type
                var genericType = temaqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is ITEMA<TPrice, TOutput> tema)
                {
                    logger?.LogDebug("Successfully created QuantConnect TEMA implementation");
                    return tema;
                }
            }
            
            logger?.LogWarning("QuantConnect TEMA type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect TEMA implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party TEMA implementation");
        return new TEMA_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements and performance benchmarking.
    /// </summary>
    private static ITEMA<TPrice, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PTEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // For TEMA, the first-party implementation is generally more optimized
        // due to efficient cascading EMA calculations and SMA seeding
        // QuantConnect implementation provides compatibility but may have overhead
        
        logger?.LogDebug("Selecting optimized TEMA implementation for period {Period}", parameters.Period);
        
        // Performance-based evaluation
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.75;
        
        // Check if QuantConnect is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific optimizations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("TEMA optimization context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, RuntimeInformation.OSDescription);

        if (quantConnectLoaded && 
            parameters.Period <= 50 && 
            !isMemoryConstrained)
        {
            // Use QuantConnect for shorter periods if already loaded and memory allows
            logger?.LogDebug("Selected QuantConnect TEMA for short-period optimization");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (parameters.Period > 200 || isMemoryConstrained)
        {
            // Use first-party for long periods or memory-constrained scenarios
            logger?.LogDebug("Selected First-Party TEMA for long-period or memory-constrained optimization");
            return new TEMA_FP<TPrice, TOutput>(parameters);
        }
        else
        {
            // Default to first-party implementation for better performance
            logger?.LogDebug("Selected First-Party TEMA as optimized default");
            return new TEMA_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions, performance characteristics, and system resources.
    /// </summary>
    private static ITEMA<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PTEMA<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - TEMA involves cascading EMAs (EMA of EMA of EMA), so efficient implementation is critical
        // - First-party uses optimized cascading with SMA seeding for better initial stability and reduced error accumulation
        // - QuantConnect provides proven compatibility but may have more overhead for complex cascading
        // - Memory usage scales with period length due to warmup requirements
        // - Performance impact varies significantly with period size and calculation frequency

        logger?.LogDebug("Selecting best TEMA implementation for period {Period}, smoothing factor {SmoothingFactor}", 
            parameters.Period, parameters.SmoothingFactor);

        // System resource analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Performance considerations based on period size
        bool isLargePeriod = parameters.Period > 100;
        bool isVeryLargePeriod = parameters.Period > 500;
        bool isExtremelyLargePeriod = parameters.Period > 1000;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific considerations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        
        logger?.LogDebug("TEMA selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Large period={LargePeriod}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, isLargePeriod, RuntimeInformation.OSDescription);

        ITEMA<TPrice, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isExtremelyLargePeriod)
        {
            // Under extreme memory pressure or extremely large periods, prioritize efficiency
            selectedImplementation = new TEMA_FP<TPrice, TOutput>(parameters);
            selectionReason = "extreme memory pressure or very large period";
        }
        else if (quantConnectLoaded && 
                 !isMemoryConstrained && 
                 parameters.Period <= 50 && 
                 !isVeryLargePeriod)
        {
            // Use QuantConnect for smaller periods when it's already loaded and memory allows
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for small period optimization";
        }
        else if (isLargePeriod || isMemoryConstrained)
        {
            // Use first-party for large periods or memory-constrained scenarios
            // FP implementation has better memory management for cascading EMAs
            selectedImplementation = new TEMA_FP<TPrice, TOutput>(parameters);
            selectionReason = "large period or memory-constrained environment";
        }
        else
        {
            // Default to first-party implementation for best performance and numerical stability
            // TEMA cascading benefits from optimized first-party implementation
            selectedImplementation = new TEMA_FP<TPrice, TOutput>(parameters);
            selectionReason = "optimal performance and numerical stability";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} TEMA implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create a TEMA indicator for double values.
    /// </summary>
    /// <param name="period">The period for the TEMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>A TEMA indicator instance for double values</returns>
    public static ITEMA<double, double> CreateDouble(int period = 14, double? smoothingFactor = null)
    {
        return Create<double, double>(period, smoothingFactor);
    }

    /// <summary>
    /// Convenience method to create a TEMA indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the TEMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>A TEMA indicator instance for decimal values</returns>
    public static ITEMA<decimal, decimal> CreateDecimal(int period = 14, decimal? smoothingFactor = null)
    {
        return Create<decimal, decimal>(period, smoothingFactor);
    }

    /// <summary>
    /// Convenience method to create a TEMA indicator for float values.
    /// </summary>
    /// <param name="period">The period for the TEMA calculation</param>
    /// <param name="smoothingFactor">Optional smoothing factor override</param>
    /// <returns>A TEMA indicator instance for float values</returns>
    public static ITEMA<float, float> CreateFloat(int period = 14, float? smoothingFactor = null)
    {
        return Create<float, float>(period, smoothingFactor);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.TEMA");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
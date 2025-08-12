using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Aroon indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class Aroon
{
    /// <summary>
    /// Creates an Aroon indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Aroon parameters</param>
    /// <returns>An Aroon indicator instance</returns>
    public static IAroon<TPrice, TOutput> Create<TPrice, TOutput>(PAroon<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new Aroon_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new Aroon_FP<TPrice, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an Aroon indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Aroon calculation (default: 14)</param>
    /// <returns>An Aroon indicator instance</returns>
    public static IAroon<TPrice, TOutput> Create<TPrice, TOutput>(int period = 14)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PAroon<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IAroon<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PAroon<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Aroon implementation for period {Period}", parameters.Period);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var aroonQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.Aroon_QC`2");
            
            if (aroonQcType != null)
            {
                // Make the generic type
                var genericType = aroonQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IAroon<TPrice, TOutput> aroon)
                {
                    logger?.LogDebug("Successfully created QuantConnect Aroon implementation");
                    return aroon;
                }
            }
            
            logger?.LogWarning("QuantConnect Aroon type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Aroon implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Aroon implementation");
        return new Aroon_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions, performance characteristics, and system resources.
    /// </summary>
    private static IAroon<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PAroon<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - Aroon requires tracking min/max positions over a sliding window (circular buffer)
        // - First-party implementation uses efficient circular buffer for O(1) operations
        // - QuantConnect may use linear search for min/max which is O(n) per calculation
        // - Memory usage is O(period) for circular buffer vs potential O(period²) for naive implementations
        // - Performance degradation is significant for large periods with inefficient algorithms

        logger?.LogDebug("Selecting best Aroon implementation for period {Period}", parameters.Period);

        // System resource analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Performance considerations based on period size (circular buffer efficiency)
        bool isSmallPeriod = parameters.Period <= 25;
        bool isMediumPeriod = parameters.Period > 25 && parameters.Period <= 100;
        bool isLargePeriod = parameters.Period > 100;
        bool isVeryLargePeriod = parameters.Period > 500;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific considerations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Aroon selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Period size={PeriodCategory}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, 
            isSmallPeriod ? "Small" : isMediumPeriod ? "Medium" : isLargePeriod ? "Large" : "VeryLarge",
            RuntimeInformation.OSDescription);

        IAroon<TPrice, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isVeryLargePeriod)
        {
            // Under extreme conditions, prioritize FP's efficient circular buffer
            selectedImplementation = new Aroon_FP<TPrice, TOutput>(parameters);
            selectionReason = "extreme memory pressure or very large period requiring efficient circular buffer";
        }
        else if (quantConnectLoaded && 
                 isSmallPeriod && 
                 !isMemoryConstrained)
        {
            // Use QuantConnect for small periods when it's already loaded and performance difference is minimal
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for small period with minimal performance impact";
        }
        else if (isMediumPeriod || isLargePeriod || isMemoryConstrained)
        {
            // Use first-party for medium to large periods where circular buffer efficiency matters
            selectedImplementation = new Aroon_FP<TPrice, TOutput>(parameters);
            selectionReason = "medium to large period benefiting from optimized circular buffer implementation";
        }
        else
        {
            // Default to first-party implementation for its superior algorithmic efficiency
            selectedImplementation = new Aroon_FP<TPrice, TOutput>(parameters);
            selectionReason = "optimal circular buffer performance and memory efficiency";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} Aroon implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create an Aroon indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Aroon calculation</param>
    /// <returns>An Aroon indicator instance for double values</returns>
    public static IAroon<double, double> CreateDouble(int period = 14)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create an Aroon indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Aroon calculation</param>
    /// <returns>An Aroon indicator instance for decimal values</returns>
    public static IAroon<decimal, decimal> CreateDecimal(int period = 14)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create an Aroon indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Aroon calculation</param>
    /// <returns>An Aroon indicator instance for float values</returns>
    public static IAroon<float, float> CreateFloat(int period = 14)
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.Aroon");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
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
/// Default Standard Deviation indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class StandardDeviation
{
    /// <summary>
    /// Creates a Standard Deviation indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Standard Deviation parameters</param>
    /// <returns>A Standard Deviation indicator instance</returns>
    public static IStandardDeviation<TPrice, TOutput> Create<TPrice, TOutput>(PStandardDeviation<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new StandardDeviation_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new StandardDeviation_FP<TPrice, TOutput>(parameters), // FP is already optimized with Welford's algorithm
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Standard Deviation indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the Standard Deviation calculation (default: 20)</param>
    /// <returns>A Standard Deviation indicator instance</returns>
    public static IStandardDeviation<TPrice, TOutput> Create<TPrice, TOutput>(int period = 20)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PStandardDeviation<TPrice, TOutput> { Period = period };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IStandardDeviation<TPrice, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PStandardDeviation<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Standard Deviation implementation for period {Period}", parameters.Period);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var stdDevQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.StandardDeviation_QC`2");
            
            if (stdDevQcType != null)
            {
                // Make the generic type
                var genericType = stdDevQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IStandardDeviation<TPrice, TOutput> stdDev)
                {
                    logger?.LogDebug("Successfully created QuantConnect Standard Deviation implementation");
                    return stdDev;
                }
            }
            
            logger?.LogWarning("QuantConnect Standard Deviation type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Standard Deviation implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Standard Deviation implementation");
        return new StandardDeviation_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions, performance characteristics, and numerical stability requirements.
    /// </summary>
    private static IStandardDeviation<TPrice, TOutput> SelectBestImplementation<TPrice, TOutput>(PStandardDeviation<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - Standard Deviation requires variance calculation with potential numerical instability
        // - First-party implementation uses Welford's online algorithm for numerical stability
        // - QuantConnect may use traditional two-pass algorithm which can have precision issues
        // - Memory usage scales with period length for buffering
        // - Numerical precision is critical especially for floating-point calculations
        // - Performance impact varies with calculation frequency and precision requirements

        logger?.LogDebug("Selecting best Standard Deviation implementation for period {Period}", parameters.Period);

        // System resource analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Performance and precision considerations based on period size
        bool isSmallPeriod = parameters.Period <= 20;
        bool isMediumPeriod = parameters.Period > 20 && parameters.Period <= 100;
        bool isLargePeriod = parameters.Period > 100;
        bool isVeryLargePeriod = parameters.Period > 500;
        
        // Numerical stability considerations based on data type
        bool requiresHighPrecision = typeof(TOutput) == typeof(decimal) || typeof(TOutput) == typeof(double);
        bool isFloatingPoint = typeof(TOutput) == typeof(float) || typeof(TOutput) == typeof(double);
        
        // Check if QuantConnect assembly is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific considerations (numerical precision varies by platform)
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Standard Deviation selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Period={Period}, Period category={PeriodCategory}, High precision={HighPrecision}, " +
            "Floating point={FloatingPoint}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, parameters.Period,
            isSmallPeriod ? "Small" : isMediumPeriod ? "Medium" : isLargePeriod ? "Large" : "VeryLarge",
            requiresHighPrecision, isFloatingPoint, RuntimeInformation.OSDescription);

        IStandardDeviation<TPrice, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isVeryLargePeriod)
        {
            // Under extreme conditions, prioritize FP's efficient Welford algorithm
            selectedImplementation = new StandardDeviation_FP<TPrice, TOutput>(parameters);
            selectionReason = "extreme memory pressure or very large period requiring efficient Welford's algorithm";
        }
        else if (requiresHighPrecision || isFloatingPoint)
        {
            // Use first-party for high precision or floating-point calculations where numerical stability is critical
            selectedImplementation = new StandardDeviation_FP<TPrice, TOutput>(parameters);
            selectionReason = "high precision or floating-point calculation requiring numerically stable Welford's algorithm";
        }
        else if (quantConnectLoaded && 
                 isSmallPeriod && 
                 !isMemoryConstrained && 
                 !requiresHighPrecision)
        {
            // Use QuantConnect for small periods when it's already loaded and precision is not critical
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for small period with acceptable numerical precision";
        }
        else if (isMediumPeriod || isLargePeriod || isMemoryConstrained)
        {
            // Use first-party for medium to large periods where Welford's algorithm efficiency matters
            selectedImplementation = new StandardDeviation_FP<TPrice, TOutput>(parameters);
            selectionReason = "medium to large period benefiting from optimized Welford's online algorithm";
        }
        else
        {
            // Default to first-party implementation for superior numerical stability and performance
            selectedImplementation = new StandardDeviation_FP<TPrice, TOutput>(parameters);
            selectionReason = "optimal numerical stability with Welford's algorithm and memory efficiency";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} Standard Deviation implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create a Standard Deviation indicator for double values.
    /// </summary>
    /// <param name="period">The period for the Standard Deviation calculation</param>
    /// <returns>A Standard Deviation indicator instance for double values</returns>
    public static IStandardDeviation<double, double> CreateDouble(int period = 20)
    {
        return Create<double, double>(period);
    }

    /// <summary>
    /// Convenience method to create a Standard Deviation indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the Standard Deviation calculation</param>
    /// <returns>A Standard Deviation indicator instance for decimal values</returns>
    public static IStandardDeviation<decimal, decimal> CreateDecimal(int period = 20)
    {
        return Create<decimal, decimal>(period);
    }

    /// <summary>
    /// Convenience method to create a Standard Deviation indicator for float values.
    /// </summary>
    /// <param name="period">The period for the Standard Deviation calculation</param>
    /// <returns>A Standard Deviation indicator instance for float values</returns>
    public static IStandardDeviation<float, float> CreateFloat(int period = 20)
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.StandardDeviation");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
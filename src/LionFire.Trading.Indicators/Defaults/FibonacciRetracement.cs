using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Fibonacci Retracement indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class FibonacciRetracement
{
    /// <summary>
    /// Creates a Fibonacci Retracement indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Fibonacci Retracement parameters</param>
    /// <returns>A Fibonacci Retracement indicator instance</returns>
    public static IFibonacciRetracement<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new FibonacciRetracement_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Fibonacci Retracement indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="lookbackPeriod">The lookback period for finding swing high/low (default: 20)</param>
    /// <param name="minSwingPercent">The minimum swing percentage to identify pivot (default: 1.0%)</param>
    /// <returns>A Fibonacci Retracement indicator instance</returns>
    public static IFibonacciRetracement<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        int lookbackPeriod = 20, 
        double minSwingPercent = 1.0)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PFibonacciRetracement<HLC<TPrice>, TOutput> 
        { 
            LookbackPeriod = lookbackPeriod,
            MinSwingPercent = minSwingPercent
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IFibonacciRetracement<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Fibonacci Retracement implementation for lookback period {LookbackPeriod}", 
                parameters.LookbackPeriod);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var fibonacciQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.FibonacciRetracement_QC`2");
            
            if (fibonacciQcType != null)
            {
                // Make the generic type
                var genericType = fibonacciQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IFibonacciRetracement<HLC<TPrice>, TOutput> fibonacci)
                {
                    logger?.LogDebug("Successfully created QuantConnect Fibonacci Retracement implementation");
                    return fibonacci;
                }
            }
            
            logger?.LogWarning("QuantConnect Fibonacci Retracement type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Fibonacci Retracement implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Fibonacci Retracement implementation");
        return new FibonacciRetracement_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static IFibonacciRetracement<HLC<TPrice>, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // For Fibonacci Retracement, the first-party implementation is highly optimized
        // due to efficient sliding window algorithm for swing point detection
        // QuantConnect implementation may have overhead for pivot point calculations
        
        logger?.LogDebug("Selecting optimized Fibonacci Retracement implementation for lookback period {LookbackPeriod}", 
            parameters.LookbackPeriod);
        
        // Memory and performance analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        
        // Check if QuantConnect is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        logger?.LogDebug("Fibonacci optimization context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Lookback={LookbackPeriod}", 
            quantConnectLoaded, isMemoryConstrained, parameters.LookbackPeriod);

        if (quantConnectLoaded && !isMemoryConstrained && parameters.LookbackPeriod <= 50)
        {
            logger?.LogDebug("Selected QuantConnect Fibonacci for optimization with moderate lookback period");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Prefer first-party for optimization, especially with large lookback periods or memory constraints
            logger?.LogDebug("Selected First-Party Fibonacci for optimized sliding window performance");
            return new FibonacciRetracement_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IFibonacciRetracement<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - Fibonacci Retracement requires efficient swing high/low detection over lookback period
        // - First-party uses optimized sliding windows for min/max tracking with swing detection
        // - Computational complexity increases with lookback period and swing detection sensitivity
        // - Memory usage scales with lookback period for historical price tracking
        // - Fibonacci ratio calculations are computationally light but pivot detection is intensive
        // - Real-time pivot detection requires efficient streaming algorithms
        
        logger?.LogDebug("Selecting best Fibonacci Retracement implementation for lookback period {LookbackPeriod}, swing threshold {MinSwingPercent}%", 
            parameters.LookbackPeriod, parameters.MinSwingPercent);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Analyze computational complexity based on parameters
        bool isSmallLookback = parameters.LookbackPeriod <= 20;
        bool isMediumLookback = parameters.LookbackPeriod > 20 && parameters.LookbackPeriod <= 100;
        bool isLargeLookback = parameters.LookbackPeriod > 100;
        bool isVeryLargeLookback = parameters.LookbackPeriod > 250;
        bool isSensitiveSwing = parameters.MinSwingPercent < 0.5;
        bool isHighComplexity = isLargeLookback || isSensitiveSwing;
        bool isVeryHighComplexity = isVeryLargeLookback && isSensitiveSwing;
        
        // Check if QuantConnect assembly is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations for intensive pivot calculations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Fibonacci selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Lookback={LookbackPeriod}, Lookback category={LookbackCategory}, Sensitive swing={SensitiveSwing}, " +
            "High complexity={HighComplexity}, Very high complexity={VeryHighComplexity}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, parameters.LookbackPeriod,
            isSmallLookback ? "Small" : isMediumLookback ? "Medium" : isLargeLookback ? "Large" : "VeryLarge",
            isSensitiveSwing, isHighComplexity, isVeryHighComplexity, RuntimeInformation.OSDescription);

        IFibonacciRetracement<HLC<TPrice>, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isVeryHighComplexity)
        {
            // Under extreme conditions, prioritize FP's efficient sliding window algorithm
            selectedImplementation = new FibonacciRetracement_FP<TPrice, TOutput>(parameters);
            selectionReason = "extreme memory pressure or very high complexity requiring efficient sliding window algorithms";
        }
        else if (quantConnectLoaded && 
                 isSmallLookback && 
                 !isSensitiveSwing && 
                 !isMemoryConstrained)
        {
            // Use QuantConnect for simple small lookback scenarios when it's already loaded
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for simple small lookback scenario with proven compatibility";
        }
        else if (isHighComplexity || isMediumLookback || isMemoryConstrained)
        {
            // Use first-party for complex scenarios where sliding window optimization matters
            selectedImplementation = new FibonacciRetracement_FP<TPrice, TOutput>(parameters);
            selectionReason = "complex pivot detection scenario benefiting from optimized sliding window algorithms";
        }
        else
        {
            // Default to first-party implementation for superior swing detection performance
            selectedImplementation = new FibonacciRetracement_FP<TPrice, TOutput>(parameters);
            selectionReason = "optimal swing detection performance with efficient sliding window implementation";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} Fibonacci Retracement implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create a Fibonacci Retracement indicator for double values.
    /// </summary>
    /// <param name="lookbackPeriod">The lookback period for finding swing high/low</param>
    /// <param name="minSwingPercent">The minimum swing percentage to identify pivot</param>
    /// <returns>A Fibonacci Retracement indicator instance for double values</returns>
    public static IFibonacciRetracement<HLC<double>, double> CreateDouble(int lookbackPeriod = 20, double minSwingPercent = 1.0)
    {
        return Create<double, double>(lookbackPeriod, minSwingPercent);
    }

    /// <summary>
    /// Convenience method to create a Fibonacci Retracement indicator for decimal values.
    /// </summary>
    /// <param name="lookbackPeriod">The lookback period for finding swing high/low</param>
    /// <param name="minSwingPercent">The minimum swing percentage to identify pivot</param>
    /// <returns>A Fibonacci Retracement indicator instance for decimal values</returns>
    public static IFibonacciRetracement<HLC<decimal>, decimal> CreateDecimal(int lookbackPeriod = 20, double minSwingPercent = 1.0)
    {
        return Create<decimal, decimal>(lookbackPeriod, minSwingPercent);
    }

    /// <summary>
    /// Convenience method to create a Fibonacci Retracement indicator for float values.
    /// </summary>
    /// <param name="lookbackPeriod">The lookback period for finding swing high/low</param>
    /// <param name="minSwingPercent">The minimum swing percentage to identify pivot</param>
    /// <returns>A Fibonacci Retracement indicator instance for float values</returns>
    public static IFibonacciRetracement<HLC<float>, float> CreateFloat(int lookbackPeriod = 20, double minSwingPercent = 1.0)
    {
        return Create<float, float>(lookbackPeriod, minSwingPercent);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.FibonacciRetracement");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}

/// <summary>
/// Legacy alias class for backward compatibility.
/// For new code, use FibonacciRetracement.Create() factory methods instead.
/// </summary>
/// <remarks>
/// This alias allows easy switching between implementations without changing client code.
/// Use FibonacciRetracement_FP from LionFire.Trading.Indicators.Native directly if needed.
/// 
/// The default points to the First-Party implementation as it's optimized for streaming data
/// with efficient swing point tracking using sliding windows for high and low values.
/// 
/// Fibonacci Retracement calculates key support and resistance levels based on the golden ratio
/// and other Fibonacci ratios, derived from the highest high and lowest low over a lookback period.
/// </remarks>
public class FibonacciRetracement<TPrice, TOutput> : FibonacciRetracement_FP<TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    public FibonacciRetracement(PFibonacciRetracement<HLC<TPrice>, TOutput> parameters) : base(parameters) { }
    
    public static new FibonacciRetracement<TPrice, TOutput> Create(PFibonacciRetracement<HLC<TPrice>, TOutput> p)
        => new FibonacciRetracement<TPrice, TOutput>(p);
}
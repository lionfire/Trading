using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Awesome Oscillator (AO) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class AwesomeOscillator
{
    /// <summary>
    /// Creates an Awesome Oscillator indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Awesome Oscillator parameters</param>
    /// <returns>An Awesome Oscillator indicator instance</returns>
    public static IAwesomeOscillator<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PAwesomeOscillator<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new AwesomeOscillator_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new AwesomeOscillator_FP<TPrice, TOutput>(parameters), // FP is already optimized with circular buffer
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an Awesome Oscillator indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="fastPeriod">The fast period for SMA calculation (default: 5)</param>
    /// <param name="slowPeriod">The slow period for SMA calculation (default: 34)</param>
    /// <returns>An Awesome Oscillator indicator instance</returns>
    public static IAwesomeOscillator<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(int fastPeriod = 5, int slowPeriod = 34)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PAwesomeOscillator<TPrice, TOutput> 
        { 
            FastPeriod = fastPeriod, 
            SlowPeriod = slowPeriod 
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IAwesomeOscillator<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PAwesomeOscillator<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Awesome Oscillator implementation for periods Fast={FastPeriod}, Slow={SlowPeriod}", 
                parameters.FastPeriod, parameters.SlowPeriod);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var aoQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.AwesomeOscillator_QC`2");
            
            if (aoQcType != null)
            {
                // Make the generic type
                var genericType = aoQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IAwesomeOscillator<HLC<TPrice>, TOutput> ao)
                {
                    logger?.LogDebug("Successfully created QuantConnect Awesome Oscillator implementation");
                    return ao;
                }
            }
            
            logger?.LogWarning("QuantConnect Awesome Oscillator type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Awesome Oscillator implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Awesome Oscillator implementation");
        return new AwesomeOscillator_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions, performance characteristics, and system resources.
    /// </summary>
    private static IAwesomeOscillator<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PAwesomeOscillator<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - Awesome Oscillator is SMA(HL/2, FastPeriod) - SMA(HL/2, SlowPeriod)
        // - Requires two concurrent SMA calculations with different periods
        // - First-party implementation uses optimized dual circular buffers
        // - QuantConnect may use less efficient buffering strategies
        // - Memory usage scales with max(FastPeriod, SlowPeriod)
        // - Performance is critical for dual SMA calculations in real-time scenarios

        logger?.LogDebug("Selecting best Awesome Oscillator implementation for periods Fast={FastPeriod}, Slow={SlowPeriod}", 
            parameters.FastPeriod, parameters.SlowPeriod);

        // System resource analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Performance considerations based on period sizes
        int maxPeriod = Math.Max(parameters.FastPeriod, parameters.SlowPeriod);
        bool isSmallPeriods = maxPeriod <= 50;
        bool isMediumPeriods = maxPeriod > 50 && maxPeriod <= 200;
        bool isLargePeriods = maxPeriod > 200;
        bool isVeryLargePeriods = maxPeriod > 500;
        
        // Dual SMA complexity analysis
        bool hasSignificantPeriodDifference = Math.Abs(parameters.SlowPeriod - parameters.FastPeriod) > 20;
        bool isDefaultConfiguration = parameters.FastPeriod == 5 && parameters.SlowPeriod == 34;
        
        // Check if QuantConnect assembly is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific considerations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("AO selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Max period={MaxPeriod}, Period category={PeriodCategory}, Default config={IsDefault}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, maxPeriod,
            isSmallPeriods ? "Small" : isMediumPeriods ? "Medium" : isLargePeriods ? "Large" : "VeryLarge",
            isDefaultConfiguration, RuntimeInformation.OSDescription);

        IAwesomeOscillator<HLC<TPrice>, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isVeryLargePeriods)
        {
            // Under extreme conditions, prioritize FP's efficient dual circular buffers
            selectedImplementation = new AwesomeOscillator_FP<TPrice, TOutput>(parameters);
            selectionReason = "extreme memory pressure or very large periods requiring efficient dual buffer management";
        }
        else if (quantConnectLoaded && 
                 isSmallPeriods && 
                 !isMemoryConstrained && 
                 isDefaultConfiguration)
        {
            // Use QuantConnect for small default configuration when it's already loaded
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for small default configuration with minimal performance impact";
        }
        else if (isMediumPeriods || isLargePeriods || isMemoryConstrained || hasSignificantPeriodDifference)
        {
            // Use first-party for medium to large periods or complex configurations
            // FP implementation optimizes dual SMA calculations better
            selectedImplementation = new AwesomeOscillator_FP<TPrice, TOutput>(parameters);
            selectionReason = "medium to large periods or complex configuration benefiting from optimized dual SMA implementation";
        }
        else
        {
            // Default to first-party implementation for superior dual SMA performance
            selectedImplementation = new AwesomeOscillator_FP<TPrice, TOutput>(parameters);
            selectionReason = "optimal dual SMA performance and memory efficiency";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} Awesome Oscillator implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create an Awesome Oscillator indicator for double values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for SMA calculation (default: 5)</param>
    /// <param name="slowPeriod">The slow period for SMA calculation (default: 34)</param>
    /// <returns>An Awesome Oscillator indicator instance for double values</returns>
    public static IAwesomeOscillator<HLC<double>, double> CreateDouble(int fastPeriod = 5, int slowPeriod = 34)
    {
        return Create<double, double>(fastPeriod, slowPeriod);
    }

    /// <summary>
    /// Convenience method to create an Awesome Oscillator indicator for decimal values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for SMA calculation (default: 5)</param>
    /// <param name="slowPeriod">The slow period for SMA calculation (default: 34)</param>
    /// <returns>An Awesome Oscillator indicator instance for decimal values</returns>
    public static IAwesomeOscillator<HLC<decimal>, decimal> CreateDecimal(int fastPeriod = 5, int slowPeriod = 34)
    {
        return Create<decimal, decimal>(fastPeriod, slowPeriod);
    }

    /// <summary>
    /// Convenience method to create an Awesome Oscillator indicator for float values.
    /// </summary>
    /// <param name="fastPeriod">The fast period for SMA calculation (default: 5)</param>
    /// <param name="slowPeriod">The slow period for SMA calculation (default: 34)</param>
    /// <returns>An Awesome Oscillator indicator instance for float values</returns>
    public static IAwesomeOscillator<HLC<float>, float> CreateFloat(int fastPeriod = 5, int slowPeriod = 34)
    {
        return Create<float, float>(fastPeriod, slowPeriod);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.AwesomeOscillator");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using LionFire.Trading;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Keltner Channels indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class KeltnerChannels
{
    /// <summary>
    /// Creates a Keltner Channels indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Keltner Channels parameters</param>
    /// <returns>A Keltner Channels indicator instance</returns>
    public static IKeltnerChannels<TInput, TOutput> Create<TInput, TOutput>(PKeltnerChannels<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new KeltnerChannels_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new KeltnerChannels_FP<TInput, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Keltner Channels indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance</returns>
    public static IKeltnerChannels<TInput, TOutput> Create<TInput, TOutput>(
        int period = 20, 
        int atrPeriod = 10, 
        TOutput? atrMultiplier = null)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PKeltnerChannels<TInput, TOutput> 
        { 
            Period = period,
            AtrPeriod = atrPeriod,
            AtrMultiplier = atrMultiplier ?? TOutput.CreateChecked(2.0)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IKeltnerChannels<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PKeltnerChannels<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Keltner Channels implementation for period: {Period}, ATR period: {AtrPeriod}", 
                parameters.Period, parameters.AtrPeriod);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var keltnerqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.KeltnerChannels_QC`2");
            
            if (keltnerqcType != null)
            {
                // Make the generic type
                var genericType = keltnerqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IKeltnerChannels<TInput, TOutput> keltner)
                {
                    logger?.LogDebug("Successfully created QuantConnect Keltner Channels implementation");
                    return keltner;
                }
            }
            
            logger?.LogWarning("QuantConnect Keltner Channels type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Keltner Channels implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Keltner Channels implementation");
        return new KeltnerChannels_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IKeltnerChannels<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PKeltnerChannels<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - Keltner Channels require EMA calculation and ATR calculation (True Range with smoothing)
        // - First-party uses optimized circular buffers for both EMA and ATR components
        // - QuantConnect has comprehensive market testing but may have overhead for dual calculations
        // - Memory usage scales with both EMA period and ATR period (two separate buffers)
        // - Performance sensitive due to multiple smoothing operations per data point
        
        logger?.LogDebug("Selecting best Keltner Channels implementation for EMA period: {Period}, ATR period: {AtrPeriod}, Multiplier: {AtrMultiplier}", 
            parameters.Period, parameters.AtrPeriod, parameters.AtrMultiplier);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool hasShortPeriods = parameters.Period < 20 && parameters.AtrPeriod < 15;
        bool hasLargePeriods = parameters.Period > 50 || parameters.AtrPeriod > 30;
        bool hasVeryLargePeriods = parameters.Period > 100 || parameters.AtrPeriod > 50;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations for dual calculations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Keltner Channels selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Short periods={ShortPeriods}, Large periods={LargePeriods}, Very large periods={VeryLargePeriods}", 
            quantConnectLoaded, isMemoryConstrained, hasShortPeriods, hasLargePeriods, hasVeryLargePeriods);

        if (quantConnectLoaded && hasVeryLargePeriods && !isMemoryConstrained)
        {
            // Use QuantConnect for very large periods when it's already loaded and memory isn't constrained
            logger?.LogDebug("Selected QuantConnect Keltner Channels for very large periods scenario");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (isMemoryConstrained || hasShortPeriods)
        {
            // Use first-party for memory-constrained or short period scenarios (most efficient with circular buffers)
            logger?.LogDebug("Selected First-Party Keltner Channels for memory-constrained or short period scenario");
            return new KeltnerChannels_FP<TInput, TOutput>(parameters);
        }
        else if (quantConnectLoaded && hasLargePeriods)
        {
            // Use QuantConnect for large periods when it's already loaded
            logger?.LogDebug("Selected QuantConnect Keltner Channels for large periods with QC already loaded");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Default to first-party for its efficiency and dual-buffer optimization
            logger?.LogDebug("Selected First-Party Keltner Channels as default choice for optimal dual-calculation performance");
            return new KeltnerChannels_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for HLC input data with double precision.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for HLC double values</returns>
    public static IKeltnerChannels<HLC<double>, double> CreateHLCDouble(int period = 20, int atrPeriod = 10, double atrMultiplier = 2.0)
    {
        return Create<HLC<double>, double>(period, atrPeriod, atrMultiplier);
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for HLC input data with decimal precision.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for HLC decimal values</returns>
    public static IKeltnerChannels<HLC<decimal>, decimal> CreateHLCDecimal(int period = 20, int atrPeriod = 10, decimal atrMultiplier = 2.0m)
    {
        return Create<HLC<decimal>, decimal>(period, atrPeriod, atrMultiplier);
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for HLC input data with float precision.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for HLC float values</returns>
    public static IKeltnerChannels<HLC<float>, float> CreateHLCFloat(int period = 20, int atrPeriod = 10, float atrMultiplier = 2.0f)
    {
        return Create<HLC<float>, float>(period, atrPeriod, atrMultiplier);
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for double values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for double values</returns>
    public static IKeltnerChannels<double, double> CreateDouble(int period = 20, int atrPeriod = 10, double atrMultiplier = 2.0)
    {
        return Create<double, double>(period, atrPeriod, atrMultiplier);
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for decimal values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for decimal values</returns>
    public static IKeltnerChannels<decimal, decimal> CreateDecimal(int period = 20, int atrPeriod = 10, decimal atrMultiplier = 2.0m)
    {
        return Create<decimal, decimal>(period, atrPeriod, atrMultiplier);
    }

    /// <summary>
    /// Convenience method to create Keltner Channels for float values.
    /// </summary>
    /// <param name="period">The period for the EMA calculation (default: 20)</param>
    /// <param name="atrPeriod">The period for the ATR calculation (default: 10)</param>
    /// <param name="atrMultiplier">The ATR multiplier for the channel bands (default: 2.0)</param>
    /// <returns>A Keltner Channels indicator instance for float values</returns>
    public static IKeltnerChannels<float, float> CreateFloat(int period = 20, int atrPeriod = 10, float atrMultiplier = 2.0f)
    {
        return Create<float, float>(period, atrPeriod, atrMultiplier);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.KeltnerChannels");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
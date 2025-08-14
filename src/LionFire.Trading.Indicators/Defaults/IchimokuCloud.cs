using LionFire.Trading;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading.Indicators.Parameters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Ichimoku Cloud indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class IchimokuCloud
{
    /// <summary>
    /// Creates an Ichimoku Cloud indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Ichimoku Cloud parameters</param>
    /// <returns>An Ichimoku Cloud indicator instance</returns>
    public static IIchimokuCloud<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(PIchimokuCloud<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new IchimokuCloud_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an Ichimoku Cloud indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="conversionLinePeriod">The conversion line period (Tenkan-sen) - default: 9</param>
    /// <param name="baseLinePeriod">The base line period (Kijun-sen) - default: 26</param>
    /// <param name="leadingSpanBPeriod">The leading span B period (Senkou Span B) - default: 52</param>
    /// <param name="displacement">The displacement for leading/lagging spans - default: 26</param>
    /// <returns>An Ichimoku Cloud indicator instance</returns>
    public static IIchimokuCloud<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        int conversionLinePeriod = 9, 
        int baseLinePeriod = 26, 
        int leadingSpanBPeriod = 52, 
        int displacement = 26)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PIchimokuCloud<TPrice, TOutput> 
        { 
            ConversionLinePeriod = conversionLinePeriod,
            BaseLinePeriod = baseLinePeriod,
            LeadingSpanBPeriod = leadingSpanBPeriod,
            Displacement = displacement
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IIchimokuCloud<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PIchimokuCloud<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Ichimoku Cloud implementation for periods: Tenkan={ConversionLinePeriod}, Kijun={BaseLinePeriod}, Senkou B={LeadingSpanBPeriod}, Displacement={Displacement}", 
                parameters.ConversionLinePeriod, parameters.BaseLinePeriod, parameters.LeadingSpanBPeriod, parameters.Displacement);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var ichimokuQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.IchimokuCloud_QC`2");
            
            if (ichimokuQcType != null)
            {
                // Make the generic type
                var genericType = ichimokuQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IIchimokuCloud<HLC<TPrice>, TOutput> ichimoku)
                {
                    logger?.LogDebug("Successfully created QuantConnect Ichimoku Cloud implementation");
                    return ichimoku;
                }
            }
            
            logger?.LogWarning("QuantConnect Ichimoku Cloud type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Ichimoku Cloud implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party Ichimoku Cloud implementation");
        return new IchimokuCloud_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static IIchimokuCloud<HLC<TPrice>, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PIchimokuCloud<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // For Ichimoku Cloud, the first-party implementation is often more optimized
        // due to efficient circular buffer usage for multiple high/low period tracking
        // QuantConnect's implementation might have additional overhead due to its generalized design
        
        logger?.LogDebug("Selecting optimized Ichimoku Cloud implementation");
        
        // Memory analysis for optimization decision
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        
        // Check if QuantConnect is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        logger?.LogDebug("Ichimoku optimization context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}", 
            quantConnectLoaded, isMemoryConstrained);

        if (quantConnectLoaded && !isMemoryConstrained)
        {
            logger?.LogDebug("Selected QuantConnect Ichimoku for optimization with abundant memory");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Prefer first-party for optimization, especially under memory constraints
            logger?.LogDebug("Selected First-Party Ichimoku for optimized circular buffer performance");
            return new IchimokuCloud_FP<TPrice, TOutput>(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IIchimokuCloud<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PIchimokuCloud<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - Ichimoku Cloud is computationally intensive requiring 5 separate calculations:
        //   1. Tenkan-sen (Conversion Line): (9-period high + low) / 2
        //   2. Kijun-sen (Base Line): (26-period high + low) / 2
        //   3. Senkou Span A (Leading Span A): (Tenkan-sen + Kijun-sen) / 2, displaced forward
        //   4. Senkou Span B (Leading Span B): (52-period high + low) / 2, displaced forward
        //   5. Chikou Span (Lagging Span): Close price displaced backward
        // - First-party uses optimized circular buffers for efficient multiple-period high/low tracking
        // - QuantConnect has comprehensive battle testing but higher memory overhead for multiple buffers
        // - Memory usage scales with longest period (typically 52) plus displacement buffers
        // - Performance critical due to multiple simultaneous period calculations
        
        logger?.LogDebug("Selecting best Ichimoku Cloud implementation for periods: Tenkan={ConversionLinePeriod}, Kijun={BaseLinePeriod}, Senkou B={LeadingSpanBPeriod}, Displacement={Displacement}", 
            parameters.ConversionLinePeriod, parameters.BaseLinePeriod, parameters.LeadingSpanBPeriod, parameters.Displacement);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        
        // Analyze computational complexity based on periods
        int maxPeriod = Math.Max(Math.Max(parameters.ConversionLinePeriod, parameters.BaseLinePeriod), parameters.LeadingSpanBPeriod);
        int totalBufferSize = maxPeriod + parameters.Displacement;
        bool isHighComplexity = maxPeriod > 50 || parameters.Displacement > 26;
        bool isVeryHighComplexity = maxPeriod > 100 || parameters.Displacement > 50;
        bool hasStandardPeriods = parameters.ConversionLinePeriod == 9 && parameters.BaseLinePeriod == 26 && 
                                 parameters.LeadingSpanBPeriod == 52 && parameters.Displacement == 26;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations for multiple buffer management
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Ichimoku selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Max period={MaxPeriod}, High complexity={HighComplexity}, Very high complexity={VeryHighComplexity}, Standard periods={StandardPeriods}", 
            quantConnectLoaded, isMemoryConstrained, maxPeriod, isHighComplexity, isVeryHighComplexity, hasStandardPeriods);

        if (quantConnectLoaded && isVeryHighComplexity && !isMemoryConstrained)
        {
            // Use QuantConnect for very complex configurations when it's already loaded and memory isn't constrained
            logger?.LogDebug("Selected QuantConnect Ichimoku for very high complexity scenario with abundant memory");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (isMemoryConstrained || !hasStandardPeriods)
        {
            // Use first-party for memory-constrained scenarios or non-standard period configurations (better flexibility)
            logger?.LogDebug("Selected First-Party Ichimoku for memory-constrained or non-standard period scenario");
            return new IchimokuCloud_FP<TPrice, TOutput>(parameters);
        }
        else if (quantConnectLoaded && isHighComplexity)
        {
            // Use QuantConnect for high complexity scenarios when it's already loaded
            logger?.LogDebug("Selected QuantConnect Ichimoku for high complexity with QC already loaded");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Default to first-party for its superior multi-buffer optimization and memory efficiency
            logger?.LogDebug("Selected First-Party Ichimoku as default choice for optimal multi-period performance");
            return new IchimokuCloud_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an Ichimoku Cloud indicator for double values.
    /// </summary>
    /// <param name="conversionLinePeriod">The conversion line period (Tenkan-sen) - default: 9</param>
    /// <param name="baseLinePeriod">The base line period (Kijun-sen) - default: 26</param>
    /// <param name="leadingSpanBPeriod">The leading span B period (Senkou Span B) - default: 52</param>
    /// <param name="displacement">The displacement for leading/lagging spans - default: 26</param>
    /// <returns>An Ichimoku Cloud indicator instance for double values</returns>
    public static IIchimokuCloud<HLC<double>, double> CreateDouble(
        int conversionLinePeriod = 9, 
        int baseLinePeriod = 26, 
        int leadingSpanBPeriod = 52, 
        int displacement = 26)
    {
        return Create<double, double>(conversionLinePeriod, baseLinePeriod, leadingSpanBPeriod, displacement);
    }

    /// <summary>
    /// Convenience method to create an Ichimoku Cloud indicator for decimal values.
    /// </summary>
    /// <param name="conversionLinePeriod">The conversion line period (Tenkan-sen) - default: 9</param>
    /// <param name="baseLinePeriod">The base line period (Kijun-sen) - default: 26</param>
    /// <param name="leadingSpanBPeriod">The leading span B period (Senkou Span B) - default: 52</param>
    /// <param name="displacement">The displacement for leading/lagging spans - default: 26</param>
    /// <returns>An Ichimoku Cloud indicator instance for decimal values</returns>
    public static IIchimokuCloud<HLC<decimal>, decimal> CreateDecimal(
        int conversionLinePeriod = 9, 
        int baseLinePeriod = 26, 
        int leadingSpanBPeriod = 52, 
        int displacement = 26)
    {
        return Create<decimal, decimal>(conversionLinePeriod, baseLinePeriod, leadingSpanBPeriod, displacement);
    }

    /// <summary>
    /// Convenience method to create an Ichimoku Cloud indicator for float values.
    /// </summary>
    /// <param name="conversionLinePeriod">The conversion line period (Tenkan-sen) - default: 9</param>
    /// <param name="baseLinePeriod">The base line period (Kijun-sen) - default: 26</param>
    /// <param name="leadingSpanBPeriod">The leading span B period (Senkou Span B) - default: 52</param>
    /// <param name="displacement">The displacement for leading/lagging spans - default: 26</param>
    /// <returns>An Ichimoku Cloud indicator instance for float values</returns>
    public static IIchimokuCloud<HLC<float>, float> CreateFloat(
        int conversionLinePeriod = 9, 
        int baseLinePeriod = 26, 
        int leadingSpanBPeriod = 52, 
        int displacement = 26)
    {
        return Create<float, float>(conversionLinePeriod, baseLinePeriod, leadingSpanBPeriod, displacement);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.IchimokuCloud");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
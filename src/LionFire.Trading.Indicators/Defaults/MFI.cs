using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Money Flow Index (MFI) indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class MFI
{
    /// <summary>
    /// Creates an MFI indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The MFI parameters</param>
    /// <returns>An MFI indicator instance</returns>
    public static IMFI<TInput, TOutput> Create<TInput, TOutput>(PMFI<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new MFI_FP<TInput, TOutput>(parameters),
            ImplementationHint.Optimized => new MFI_FP<TInput, TOutput>(parameters), // FP is already optimized with circular buffers
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates an MFI indicator with default parameters.
    /// </summary>
    /// <typeparam name="TInput">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="period">The period for the MFI calculation (default: 14)</param>
    /// <param name="overboughtLevel">The overbought threshold (default: 80)</param>
    /// <param name="oversoldLevel">The oversold threshold (default: 20)</param>
    /// <returns>An MFI indicator instance</returns>
    public static IMFI<TInput, TOutput> Create<TInput, TOutput>(
        int period = 14, 
        TOutput? overboughtLevel = null, 
        TOutput? oversoldLevel = null)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PMFI<TInput, TOutput> 
        { 
            Period = period,
            OverboughtLevel = overboughtLevel ?? TOutput.CreateChecked(80),
            OversoldLevel = oversoldLevel ?? TOutput.CreateChecked(20)
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IMFI<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PMFI<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect MFI implementation for period: {Period}", parameters.Period);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var mfiqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.MFI_QC`2");
            
            if (mfiqcType != null)
            {
                // Make the generic type
                var genericType = mfiqcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IMFI<TInput, TOutput> mfi)
                {
                    logger?.LogDebug("Successfully created QuantConnect MFI implementation");
                    return mfi;
                }
            }
            
            logger?.LogWarning("QuantConnect MFI type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect MFI implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party MFI implementation");
        return new MFI_FP<TInput, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IMFI<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PMFI<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // Decision logic for selecting implementation:
        // - MFI requires both price and volume calculations with positive/negative money flow tracking
        // - First-party uses circular buffers for efficient memory usage with streaming updates
        // - QuantConnect has extensive market testing but may have higher memory overhead
        // - Memory usage scales with period size (three circular buffers: typical prices, pos/neg flows)
        // - Performance critical for real-time volume-weighted analysis
        
        logger?.LogDebug("Selecting best MFI implementation for period: {Period}, Overbought: {OverboughtLevel}, Oversold: {OversoldLevel}", 
            parameters.Period, parameters.OverboughtLevel, parameters.OversoldLevel);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isShortPeriod = parameters.Period < 20;
        bool isLargePeriod = parameters.Period > 50;
        bool isVeryLargePeriod = parameters.Period > 100;
        
        // Check if QuantConnect assembly is already loaded (to avoid loading it unnecessarily)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations - MFI involves more floating point operations
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("MFI selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, Short period={ShortPeriod}, Large period={LargePeriod}, Very large period={VeryLargePeriod}", 
            quantConnectLoaded, isMemoryConstrained, isShortPeriod, isLargePeriod, isVeryLargePeriod);

        if (quantConnectLoaded && isVeryLargePeriod && !isMemoryConstrained)
        {
            // Use QuantConnect for very large periods when it's already loaded and memory isn't constrained
            logger?.LogDebug("Selected QuantConnect MFI for very large period scenario with abundant memory");
            return CreateQuantConnectImplementation(parameters);
        }
        else if (isMemoryConstrained || isShortPeriod)
        {
            // Use first-party for memory-constrained or short period scenarios (most efficient with circular buffers)
            logger?.LogDebug("Selected First-Party MFI for memory-constrained or short period scenario");
            return new MFI_FP<TInput, TOutput>(parameters);
        }
        else if (quantConnectLoaded && isLargePeriod)
        {
            // Use QuantConnect for large periods when it's already loaded
            logger?.LogDebug("Selected QuantConnect MFI for large period with QC already loaded");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Default to first-party for its efficiency and optimized memory usage with volume calculations
            logger?.LogDebug("Selected First-Party MFI as default choice for optimal volume-weighted performance");
            return new MFI_FP<TInput, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create an MFI indicator for double values.
    /// </summary>
    /// <param name="period">The period for the MFI calculation</param>
    /// <param name="overboughtLevel">The overbought threshold (default: 80)</param>
    /// <param name="oversoldLevel">The oversold threshold (default: 20)</param>
    /// <returns>An MFI indicator instance for double values</returns>
    public static IMFI<double, double> CreateDouble(int period = 14, double overboughtLevel = 80.0, double oversoldLevel = 20.0)
    {
        return Create<double, double>(period, overboughtLevel, oversoldLevel);
    }

    /// <summary>
    /// Convenience method to create an MFI indicator for decimal values.
    /// </summary>
    /// <param name="period">The period for the MFI calculation</param>
    /// <param name="overboughtLevel">The overbought threshold (default: 80)</param>
    /// <param name="oversoldLevel">The oversold threshold (default: 20)</param>
    /// <returns>An MFI indicator instance for decimal values</returns>
    public static IMFI<decimal, decimal> CreateDecimal(int period = 14, decimal overboughtLevel = 80m, decimal oversoldLevel = 20m)
    {
        return Create<decimal, decimal>(period, overboughtLevel, oversoldLevel);
    }

    /// <summary>
    /// Convenience method to create an MFI indicator for float values.
    /// </summary>
    /// <param name="period">The period for the MFI calculation</param>
    /// <param name="overboughtLevel">The overbought threshold (default: 80)</param>
    /// <param name="oversoldLevel">The oversold threshold (default: 20)</param>
    /// <returns>An MFI indicator instance for float values</returns>
    public static IMFI<float, float> CreateFloat(int period = 14, float overboughtLevel = 80.0f, float oversoldLevel = 20.0f)
    {
        return Create<float, float>(period, overboughtLevel, oversoldLevel);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.MFI");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
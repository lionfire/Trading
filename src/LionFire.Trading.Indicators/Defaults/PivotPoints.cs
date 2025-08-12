using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Pivot Points indicator factory with intelligent implementation selection
/// </summary>
/// <remarks>
/// This factory automatically selects the best available implementation:
/// 1. First-Party (PivotPoints_FP) - Default, optimized for streaming data
/// 2. QuantConnect (PivotPoints_QC) - When explicitly requested via ImplementationHint
/// 
/// For direct access to specific implementations:
/// - Use PivotPoints_FP from LionFire.Trading.Indicators.Native for first-party
/// - Use PivotPoints_QC from LionFire.Trading.Indicators.QuantConnect for QuantConnect wrapper
/// </remarks>
public static class PivotPoints
{
    /// <summary>
    /// Create a Pivot Points indicator with the specified parameters
    /// </summary>
    /// <param name="parameters">The pivot points parameters</param>
    /// <returns>A pivot points indicator instance</returns>
    public static IPivotPoints<TInput, TOutput> Create<TInput, TOutput>(PPivotPoints<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        return SelectBestImplementation(parameters);
    }

    /// <summary>
    /// Create a Pivot Points indicator with default parameters
    /// </summary>
    /// <param name="periodType">The period type for pivot calculation (default: Daily)</param>
    /// <returns>A pivot points indicator instance</returns>
    public static IPivotPoints<TInput, TOutput> Create<TInput, TOutput>(PivotPointsPeriod periodType = PivotPointsPeriod.Daily)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PPivotPoints<TInput, TOutput>
        {
            PeriodType = periodType
        };
        return Create(parameters);
    }

    /// <summary>
    /// Selects the best implementation based on runtime conditions, performance characteristics, and system resources.
    /// </summary>
    private static IPivotPoints<TInput, TOutput> SelectBestImplementation<TInput, TOutput>(PPivotPoints<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - Pivot Points calculations are time-based and require period boundary detection
        // - First-party implementation uses optimized time-based caching and boundary detection
        // - QuantConnect may have overhead for time zone handling and period transitions
        // - Memory usage varies based on period type and historical data retention
        // - Performance is critical for intraday pivot calculations with frequent updates

        logger?.LogDebug("Selecting best Pivot Points implementation for period type {PeriodType}", parameters.PeriodType);

        // Check implementation hint preference first
        if (parameters.ImplementationHint == ImplementationHint.QuantConnect)
        {
            try
            {
                logger?.LogDebug("QuantConnect implementation explicitly requested");
                return CreateQuantConnectImplementation(parameters);
            }
            catch (Exception ex)
            {
                logger?.LogWarning("QuantConnect implementation requested but failed: {Message}, falling back to First-Party", ex.Message);
                // Fall back to first-party
            }
        }
        else if (parameters.ImplementationHint == ImplementationHint.FirstParty)
        {
            logger?.LogDebug("First-Party implementation explicitly requested");
            return CreateFirstPartyImplementation(parameters);
        }

        // Auto-selection logic for best performance
        // System resource analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Period type complexity analysis
        bool isHighFrequencyPeriod = parameters.PeriodType == PivotPointsPeriod.Minute || 
                                    parameters.PeriodType == PivotPointsPeriod.Hourly;
        bool isStandardPeriod = parameters.PeriodType == PivotPointsPeriod.Daily || 
                              parameters.PeriodType == PivotPointsPeriod.Weekly;
        bool isComplexPeriod = parameters.PeriodType == PivotPointsPeriod.Monthly || 
                              parameters.PeriodType == PivotPointsPeriod.Quarterly;
        
        // Check if QuantConnect assembly is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        // Platform-specific considerations (timezone handling varies by platform)
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("Pivot Points selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Period type={PeriodType}, High freq={HighFrequency}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, parameters.PeriodType, isHighFrequencyPeriod, RuntimeInformation.OSDescription);

        IPivotPoints<TInput, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory)
        {
            // Under extreme memory pressure, prioritize FP's efficient caching
            selectedImplementation = CreateFirstPartyImplementation(parameters);
            selectionReason = "extreme memory pressure requiring efficient period caching";
        }
        else if (quantConnectLoaded && 
                 isStandardPeriod && 
                 !isMemoryConstrained)
        {
            // Use QuantConnect for standard periods when it's already loaded
            try
            {
                selectedImplementation = CreateQuantConnectImplementation(parameters);
                selectionReason = "QuantConnect available for standard period with proven compatibility";
            }
            catch
            {
                selectedImplementation = CreateFirstPartyImplementation(parameters);
                selectionReason = "QuantConnect failed, using First-Party fallback";
            }
        }
        else if (isHighFrequencyPeriod || isComplexPeriod || isMemoryConstrained)
        {
            // Use first-party for high-frequency or complex periods
            // FP implementation has better performance for frequent calculations and complex period handling
            selectedImplementation = CreateFirstPartyImplementation(parameters);
            selectionReason = "high-frequency or complex period benefiting from optimized time-based calculations";
        }
        else
        {
            // Default to first-party implementation for superior performance and reliability
            selectedImplementation = CreateFirstPartyImplementation(parameters);
            selectionReason = "optimal time-based calculation performance and memory efficiency";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} Pivot Points implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IPivotPoints<TInput, TOutput> CreateQuantConnectImplementation<TInput, TOutput>(PPivotPoints<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect Pivot Points implementation for period type {PeriodType}", 
                parameters.PeriodType);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var pivotPointsQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.PivotPoints_QC`2");
            
            if (pivotPointsQcType != null)
            {
                // Make the generic type
                var genericType = pivotPointsQcType.MakeGenericType(typeof(TInput), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IPivotPoints<TInput, TOutput> pivotPoints)
                {
                    logger?.LogDebug("Successfully created QuantConnect Pivot Points implementation");
                    return pivotPoints;
                }
            }
            
            logger?.LogWarning("QuantConnect Pivot Points type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
            throw new InvalidOperationException("QuantConnect implementation requested but not available. Install QuantConnect packages or use FirstParty implementation.", ex);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect Pivot Points implementation: {Message}", ex.Message);
            throw new InvalidOperationException($"Failed to create QuantConnect PivotPoints implementation: {ex.Message}", ex);
        }
        
        throw new InvalidOperationException("Failed to create QuantConnect PivotPoints implementation");
    }

    /// <summary>
    /// Creates the first-party implementation with optimized performance characteristics.
    /// </summary>
    private static IPivotPoints<TInput, TOutput> CreateFirstPartyImplementation<TInput, TOutput>(PPivotPoints<TInput, TOutput> parameters)
        where TInput : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        logger?.LogDebug("Creating First-Party Pivot Points implementation for period type {PeriodType}", parameters.PeriodType);
        
        try
        {
            var implementation = Native.PivotPoints_FP<TInput, TOutput>.Create(parameters);
            logger?.LogDebug("Successfully created First-Party Pivot Points implementation");
            return implementation;
        }
        catch (Exception ex)
        {
            logger?.LogError("Failed to create First-Party Pivot Points implementation: {Message}", ex.Message);
            throw;
        }
    }

    #region Convenience Methods

    /// <summary>
    /// Create a daily Pivot Points indicator for double values
    /// </summary>
    /// <returns>A pivot points indicator instance for double values</returns>
    public static IPivotPoints<double, double> CreateDouble(PivotPointsPeriod periodType = PivotPointsPeriod.Daily)
    {
        return Create<double, double>(periodType);
    }

    /// <summary>
    /// Create a daily Pivot Points indicator for decimal values
    /// </summary>
    /// <returns>A pivot points indicator instance for decimal values</returns>
    public static IPivotPoints<decimal, decimal> CreateDecimal(PivotPointsPeriod periodType = PivotPointsPeriod.Daily)
    {
        return Create<decimal, decimal>(periodType);
    }

    /// <summary>
    /// Create a daily Pivot Points indicator for float values
    /// </summary>
    /// <returns>A pivot points indicator instance for float values</returns>
    public static IPivotPoints<float, float> CreateFloat(PivotPointsPeriod periodType = PivotPointsPeriod.Daily)
    {
        return Create<float, float>(periodType);
    }

    #endregion
    
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.PivotPoints");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}
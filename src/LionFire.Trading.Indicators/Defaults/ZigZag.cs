using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators;
using LionFire.Trading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default ZigZag indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// </summary>
public static class ZigZag
{
    /// <summary>
    /// Creates a ZigZag indicator with default parameters
    /// </summary>
    /// <typeparam name="TPrice">Input price type</typeparam>
    /// <typeparam name="TOutput">Output value type</typeparam>
    /// <returns>ZigZag indicator instance</returns>
    public static IZigZag<HLC<TPrice>, TOutput> Create<TPrice, TOutput>()
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PZigZag<HLC<TPrice>, TOutput>();
        return Create(parameters);
    }

    /// <summary>
    /// Creates a ZigZag indicator with specified deviation and depth
    /// </summary>
    /// <typeparam name="TPrice">Input price type</typeparam>
    /// <typeparam name="TOutput">Output value type</typeparam>
    /// <param name="deviation">Minimum percentage deviation (default: 5.0%)</param>
    /// <param name="depth">Minimum bars between pivots (default: 12)</param>
    /// <returns>ZigZag indicator instance</returns>
    public static IZigZag<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        TOutput deviation, 
        int depth = 12)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PZigZag<HLC<TPrice>, TOutput>
        {
            Deviation = deviation,
            Depth = depth
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a ZigZag indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">Input price type</typeparam>
    /// <typeparam name="TOutput">Output value type</typeparam>
    /// <param name="parameters">ZigZag parameters</param>
    /// <returns>ZigZag indicator instance</returns>
    public static IZigZag<HLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        PZigZag<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.PreferredImplementation switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => ZigZag_FP<TPrice, TOutput>.Create(parameters),
            ImplementationHint.Optimized => SelectOptimizedImplementation(parameters),
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// </summary>
    private static IZigZag<HLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PZigZag<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        try
        {
            logger?.LogDebug("Attempting to create QuantConnect ZigZag implementation for deviation {Deviation}%, depth {Depth}", 
                parameters.Deviation, parameters.Depth);
            
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var zigZagQcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.ZigZag_QC`2");
            
            if (zigZagQcType != null)
            {
                // Make the generic type
                var genericType = zigZagQcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is IZigZag<HLC<TPrice>, TOutput> zigZag)
                {
                    logger?.LogDebug("Successfully created QuantConnect ZigZag implementation");
                    return zigZag;
                }
            }
            
            logger?.LogWarning("QuantConnect ZigZag type not found in assembly");
        }
        catch (FileNotFoundException ex)
        {
            logger?.LogDebug("QuantConnect assembly not found: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            logger?.LogWarning("Failed to create QuantConnect ZigZag implementation: {Message}", ex.Message);
        }
        
        // Fallback to first-party implementation
        logger?.LogDebug("Falling back to First-Party ZigZag implementation");
        return ZigZag_FP<TPrice, TOutput>.Create(parameters);
    }
    
    /// <summary>
    /// Selects the optimized implementation based on the specific requirements.
    /// </summary>
    private static IZigZag<HLC<TPrice>, TOutput> SelectOptimizedImplementation<TPrice, TOutput>(PZigZag<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        
        // For ZigZag, the first-party implementation is typically more optimized
        // due to efficient state machine implementation for pivot detection
        // QuantConnect implementation may have additional overhead for compatibility
        
        logger?.LogDebug("Selecting optimized ZigZag implementation for deviation {Deviation}%, depth {Depth}", 
            parameters.Deviation, parameters.Depth);
        
        // Memory and performance analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        
        // Check if QuantConnect is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        logger?.LogDebug("ZigZag optimization context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Deviation={Deviation}%, Depth={Depth}", 
            quantConnectLoaded, isMemoryConstrained, parameters.Deviation, parameters.Depth);

        if (quantConnectLoaded && !isMemoryConstrained)
        {
            logger?.LogDebug("Selected QuantConnect ZigZag for optimization with abundant memory");
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Prefer first-party for optimization, especially with memory constraints
            logger?.LogDebug("Selected First-Party ZigZag for optimized state machine performance");
            return ZigZag_FP<TPrice, TOutput>.Create(parameters);
        }
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static IZigZag<HLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PZigZag<HLC<TPrice>, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var logger = GetLogger();
        var stopwatch = Stopwatch.StartNew();
        
        // Decision logic for selecting implementation:
        // - ZigZag requires complex state management for pivot detection and reversal confirmation
        // - First-party uses optimized state machine for efficient pivot tracking
        // - Computational complexity varies with deviation sensitivity and depth requirements
        // - Memory usage is generally low but depends on historical pivot tracking
        // - Real-time pivot confirmation requires efficient buffering and state transitions
        // - Lower deviation values require more sensitive pivot detection (higher computational load)
        // - Higher depth values require more historical state tracking (higher memory usage)
        
        logger?.LogDebug("Selecting best ZigZag implementation for deviation {Deviation}%, depth {Depth}", 
            parameters.Deviation, parameters.Depth);

        // Performance and memory analysis
        var memoryInfo = GC.GetGCMemoryInfo();
        bool isMemoryConstrained = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.8;
        bool isLowMemory = memoryInfo.MemoryLoadBytes > memoryInfo.TotalAvailableMemoryBytes * 0.9;
        
        // Analyze computational complexity based on parameters
        var deviationValue = Convert.ToDouble(parameters.Deviation);
        bool isHighSensitivity = deviationValue < 2.0; // Very sensitive pivot detection
        bool isMediumSensitivity = deviationValue >= 2.0 && deviationValue <= 5.0; // Standard sensitivity
        bool isLowSensitivity = deviationValue > 5.0; // Less sensitive, fewer pivots
        bool isShallowDepth = parameters.Depth <= 5; // Minimal lookback
        bool isMediumDepth = parameters.Depth > 5 && parameters.Depth <= 20; // Standard lookback
        bool isDeepDepth = parameters.Depth > 20; // Extended lookback for confirmation
        bool isHighComplexity = isHighSensitivity || isDeepDepth;
        bool isVeryHighComplexity = isHighSensitivity && isDeepDepth;
        
        // Check if QuantConnect assembly is already loaded
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);
        
        // Platform-specific considerations for state machine performance
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        logger?.LogDebug("ZigZag selection context: QC loaded={QuantConnectLoaded}, Memory constrained={MemoryConstrained}, " +
            "Low memory={LowMemory}, Deviation={Deviation}%, Depth={Depth}, Sensitivity={Sensitivity}, " +
            "Depth category={DepthCategory}, High complexity={HighComplexity}, Very high complexity={VeryHighComplexity}, Platform={Platform}", 
            quantConnectLoaded, isMemoryConstrained, isLowMemory, parameters.Deviation, parameters.Depth,
            isHighSensitivity ? "High" : isMediumSensitivity ? "Medium" : "Low",
            isShallowDepth ? "Shallow" : isMediumDepth ? "Medium" : "Deep",
            isHighComplexity, isVeryHighComplexity, RuntimeInformation.OSDescription);

        IZigZag<HLC<TPrice>, TOutput> selectedImplementation;
        string selectionReason;

        if (isLowMemory || isVeryHighComplexity)
        {
            // Under extreme conditions, prioritize FP's efficient state machine
            selectedImplementation = ZigZag_FP<TPrice, TOutput>.Create(parameters);
            selectionReason = "extreme memory pressure or very high complexity requiring efficient state machine implementation";
        }
        else if (quantConnectLoaded && 
                 isLowSensitivity && 
                 isMediumDepth && 
                 !isMemoryConstrained)
        {
            // Use QuantConnect for simple low sensitivity scenarios when it's already loaded
            selectedImplementation = CreateQuantConnectImplementation(parameters);
            selectionReason = "QuantConnect available for simple low sensitivity scenario with proven compatibility";
        }
        else if (isHighComplexity || isMemoryConstrained)
        {
            // Use first-party for complex scenarios where state machine optimization matters
            selectedImplementation = ZigZag_FP<TPrice, TOutput>.Create(parameters);
            selectionReason = "complex pivot detection scenario benefiting from optimized state machine implementation";
        }
        else
        {
            // Default to first-party implementation for superior pivot detection performance
            selectedImplementation = ZigZag_FP<TPrice, TOutput>.Create(parameters);
            selectionReason = "optimal pivot detection performance with efficient state machine implementation";
        }
        
        stopwatch.Stop();
        logger?.LogDebug("Selected {ImplementationType} ZigZag implementation due to {Reason} (selection took {ElapsedMs}ms)", 
            selectedImplementation.GetType().Name, selectionReason, stopwatch.ElapsedMilliseconds);
            
        return selectedImplementation;
    }

    /// <summary>
    /// Convenience method to create a ZigZag indicator for double values.
    /// </summary>
    /// <param name="deviation">Minimum percentage deviation</param>
    /// <param name="depth">Minimum bars between pivots</param>
    /// <returns>A ZigZag indicator instance for double values</returns>
    public static IZigZag<HLC<double>, double> CreateDouble(double deviation = 5.0, int depth = 12)
    {
        return Create<double, double>(deviation, depth);
    }

    /// <summary>
    /// Convenience method to create a ZigZag indicator for decimal values.
    /// </summary>
    /// <param name="deviation">Minimum percentage deviation</param>
    /// <param name="depth">Minimum bars between pivots</param>
    /// <returns>A ZigZag indicator instance for decimal values</returns>
    public static IZigZag<HLC<decimal>, decimal> CreateDecimal(decimal deviation = 5.0m, int depth = 12)
    {
        return Create<decimal, decimal>(deviation, depth);
    }

    /// <summary>
    /// Convenience method to create a ZigZag indicator for float values.
    /// </summary>
    /// <param name="deviation">Minimum percentage deviation</param>
    /// <param name="depth">Minimum bars between pivots</param>
    /// <returns>A ZigZag indicator instance for float values</returns>
    public static IZigZag<HLC<float>, float> CreateFloat(float deviation = 5.0f, int depth = 12)
    {
        return Create<float, float>(deviation, depth);
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
            return loggerFactory?.CreateLogger("LionFire.Trading.Indicators.ZigZag");
        }
        catch
        {
            // Logging is optional - don't break if it's not available
            return null;
        }
    }
}

/// <summary>
/// Strongly-typed ZigZag indicator for common decimal types
/// </summary>
public static class ZigZag<TPrice>
    where TPrice : struct
{
    /// <summary>
    /// Creates a ZigZag indicator with decimal output
    /// </summary>
    public static IZigZag<HLC<TPrice>, decimal> Create(decimal deviation = 5.0m, int depth = 12)
        => ZigZag.Create<TPrice, decimal>(deviation, depth);

    /// <summary>
    /// Creates a ZigZag indicator with double output
    /// </summary>
    public static IZigZag<HLC<TPrice>, double> CreateDouble(double deviation = 5.0, int depth = 12)
        => ZigZag.Create<TPrice, double>(deviation, depth);

    /// <summary>
    /// Creates a ZigZag indicator with float output
    /// </summary>
    public static IZigZag<HLC<TPrice>, float> CreateFloat(float deviation = 5.0f, int depth = 12)
        => ZigZag.Create<TPrice, float>(deviation, depth);
}

/// <summary>
/// Most common ZigZag indicator variations
/// </summary>
public static class ZigZagCommon
{
    /// <summary>
    /// Creates a decimal ZigZag indicator with decimal input/output
    /// </summary>
    public static IZigZag<HLC<decimal>, decimal> Create(decimal deviation = 5.0m, int depth = 12)
        => ZigZag<decimal>.Create(deviation, depth);

    /// <summary>
    /// Creates a double ZigZag indicator with double input/output
    /// </summary>
    public static IZigZag<HLC<double>, double> CreateDouble(double deviation = 5.0, int depth = 12)
        => ZigZag<double>.CreateDouble(deviation, depth);

    /// <summary>
    /// Creates a float ZigZag indicator with float input/output
    /// </summary>
    public static IZigZag<HLC<float>, float> CreateFloat(float deviation = 5.0f, int depth = 12)
        => ZigZag<float>.CreateFloat(deviation, depth);
}
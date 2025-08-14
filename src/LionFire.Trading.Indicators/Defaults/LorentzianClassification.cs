using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.DataFlow.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.Defaults;

/// <summary>
/// Default Lorentzian Classification indicator factory.
/// Automatically selects the best implementation based on the ImplementationHint parameter.
/// 
/// The Lorentzian Classification indicator is an advanced ML-based indicator that uses 
/// k-NN with Lorentzian distance metric for market direction prediction. It extracts
/// multiple features (RSI, CCI, ADX, etc.) and classifies patterns based on historical data.
/// </summary>
public static class LorentzianClassification
{
    /// <summary>
    /// Creates a Lorentzian Classification indicator with the specified parameters, automatically selecting the best implementation.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="parameters">The Lorentzian Classification parameters</param>
    /// <returns>A Lorentzian Classification indicator instance</returns>
    public static ILorentzianClassification<OHLC<TPrice>, TOutput> Create<TPrice, TOutput>(PLorentzianClassification<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        return parameters.ImplementationHint switch
        {
            ImplementationHint.QuantConnect => CreateQuantConnectImplementation(parameters),
            ImplementationHint.FirstParty => new LorentzianClassification_FP<TPrice, TOutput>(parameters),
            ImplementationHint.Optimized => new LorentzianClassification_FP<TPrice, TOutput>(parameters), // FP is already optimized
            ImplementationHint.Auto => SelectBestImplementation(parameters),
            _ => SelectBestImplementation(parameters)
        };
    }

    /// <summary>
    /// Creates a Lorentzian Classification indicator with default parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="neighborsCount">The number of nearest neighbors to consider (default: 8)</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns (default: 100)</param>
    /// <param name="normalizationWindow">The window size for feature normalization (default: 20)</param>
    /// <returns>A Lorentzian Classification indicator instance</returns>
    public static ILorentzianClassification<OHLC<TPrice>, TOutput> Create<TPrice, TOutput>(
        int neighborsCount = 8, 
        int lookbackPeriod = 100, 
        int normalizationWindow = 20)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PLorentzianClassification<TPrice, TOutput> 
        { 
            NeighborsCount = neighborsCount,
            LookbackPeriod = lookbackPeriod,
            NormalizationWindow = normalizationWindow
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a QuantConnect implementation using reflection to avoid direct dependency.
    /// Note: Currently only FirstParty implementation is available for this custom indicator.
    /// </summary>
    private static ILorentzianClassification<OHLC<TPrice>, TOutput> CreateQuantConnectImplementation<TPrice, TOutput>(PLorentzianClassification<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        try
        {
            // Try to load the QuantConnect implementation assembly
            var qcAssembly = Assembly.Load("LionFire.Trading.Indicators.QuantConnect");
            var lcqcType = qcAssembly.GetType($"LionFire.Trading.Indicators.QuantConnect_.LorentzianClassification_QC`2");
            
            if (lcqcType != null)
            {
                // Make the generic type
                var genericType = lcqcType.MakeGenericType(typeof(TPrice), typeof(TOutput));
                
                // Create an instance
                var instance = Activator.CreateInstance(genericType, parameters);
                
                if (instance is ILorentzianClassification<OHLC<TPrice>, TOutput> lc)
                {
                    return lc;
                }
            }
        }
        catch
        {
            // Fall back to first-party implementation if QuantConnect is not available
        }
        
        // Fallback to first-party implementation
        return new LorentzianClassification_FP<TPrice, TOutput>(parameters);
    }
    
    /// <summary>
    /// Selects the best implementation based on runtime conditions and performance characteristics.
    /// </summary>
    private static ILorentzianClassification<OHLC<TPrice>, TOutput> SelectBestImplementation<TPrice, TOutput>(PLorentzianClassification<TPrice, TOutput> parameters)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        // Decision logic for selecting implementation:
        // - The Lorentzian Classification is a complex ML algorithm that benefits from optimized implementation
        // - The first-party implementation is highly optimized with circular buffers and efficient feature extraction
        // - For this custom indicator, we always use the first-party implementation as it's the most complete

        // Check if QuantConnect assembly is already loaded (though LC may not be available there)
        bool quantConnectLoaded = AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => a.FullName?.StartsWith("LionFire.Trading.Indicators.QuantConnect") == true);

        if (quantConnectLoaded)
        {
            // Try QuantConnect first if loaded, but will likely fall back to FP
            return CreateQuantConnectImplementation(parameters);
        }
        else
        {
            // Use first-party implementation (recommended for Lorentzian Classification)
            return new LorentzianClassification_FP<TPrice, TOutput>(parameters);
        }
    }

    /// <summary>
    /// Convenience method to create a Lorentzian Classification indicator for double values.
    /// </summary>
    /// <param name="neighborsCount">The number of nearest neighbors to consider (default: 8)</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns (default: 100)</param>
    /// <param name="normalizationWindow">The window size for feature normalization (default: 20)</param>
    /// <returns>A Lorentzian Classification indicator instance for double values</returns>
    public static ILorentzianClassification<OHLC<double>, double> CreateDouble(
        int neighborsCount = 8, 
        int lookbackPeriod = 100, 
        int normalizationWindow = 20)
    {
        return Create<double, double>(neighborsCount, lookbackPeriod, normalizationWindow);
    }

    /// <summary>
    /// Convenience method to create a Lorentzian Classification indicator for decimal values.
    /// </summary>
    /// <param name="neighborsCount">The number of nearest neighbors to consider (default: 8)</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns (default: 100)</param>
    /// <param name="normalizationWindow">The window size for feature normalization (default: 20)</param>
    /// <returns>A Lorentzian Classification indicator instance for decimal values</returns>
    public static ILorentzianClassification<OHLC<decimal>, decimal> CreateDecimal(
        int neighborsCount = 8, 
        int lookbackPeriod = 100, 
        int normalizationWindow = 20)
    {
        return Create<decimal, decimal>(neighborsCount, lookbackPeriod, normalizationWindow);
    }

    /// <summary>
    /// Convenience method to create a Lorentzian Classification indicator for float values.
    /// </summary>
    /// <param name="neighborsCount">The number of nearest neighbors to consider (default: 8)</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns (default: 100)</param>
    /// <param name="normalizationWindow">The window size for feature normalization (default: 20)</param>
    /// <returns>A Lorentzian Classification indicator instance for float values</returns>
    public static ILorentzianClassification<OHLC<float>, float> CreateFloat(
        int neighborsCount = 8, 
        int lookbackPeriod = 100, 
        int normalizationWindow = 20)
    {
        return Create<float, float>(neighborsCount, lookbackPeriod, normalizationWindow);
    }

    /// <summary>
    /// Creates a Lorentzian Classification indicator optimized for backtesting with specific parameters.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="neighborsCount">The number of nearest neighbors to consider</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns</param>
    /// <param name="minConfidence">Minimum confidence required to generate signals</param>
    /// <param name="labelThreshold">Threshold for labeling patterns as bullish/bearish</param>
    /// <returns>A Lorentzian Classification indicator instance optimized for backtesting</returns>
    public static ILorentzianClassification<OHLC<TPrice>, TOutput> CreateForBacktesting<TPrice, TOutput>(
        int neighborsCount = 8,
        int lookbackPeriod = 200, // Longer lookback for backtesting
        double minConfidence = 0.7, // Higher confidence for backtesting
        double labelThreshold = 0.015) // 1.5% threshold
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PLorentzianClassification<TPrice, TOutput> 
        { 
            NeighborsCount = neighborsCount,
            LookbackPeriod = lookbackPeriod,
            MinConfidence = minConfidence,
            LabelThreshold = labelThreshold,
            NormalizationWindow = Math.Min(50, lookbackPeriod / 4) // Adaptive normalization window
        };
        return Create(parameters);
    }

    /// <summary>
    /// Creates a Lorentzian Classification indicator optimized for live trading with faster response.
    /// </summary>
    /// <typeparam name="TPrice">The input price type</typeparam>
    /// <typeparam name="TOutput">The output value type</typeparam>
    /// <param name="neighborsCount">The number of nearest neighbors to consider</param>
    /// <param name="lookbackPeriod">The lookback period for historical patterns (shorter for live trading)</param>
    /// <param name="minConfidence">Minimum confidence required to generate signals</param>
    /// <returns>A Lorentzian Classification indicator instance optimized for live trading</returns>
    public static ILorentzianClassification<OHLC<TPrice>, TOutput> CreateForLiveTrading<TPrice, TOutput>(
        int neighborsCount = 5, // Fewer neighbors for faster response
        int lookbackPeriod = 100,
        double minConfidence = 0.6)
        where TPrice : struct
        where TOutput : struct, INumber<TOutput>
    {
        var parameters = new PLorentzianClassification<TPrice, TOutput> 
        { 
            NeighborsCount = neighborsCount,
            LookbackPeriod = lookbackPeriod,
            MinConfidence = minConfidence,
            NormalizationWindow = 20, // Shorter normalization for responsiveness
            LabelLookahead = 3 // Shorter lookahead for live trading
        };
        return Create(parameters);
    }
}
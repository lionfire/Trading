using System.Numerics;

namespace LionFire.Trading.Indicators;

/// <summary>
/// Lorentzian Classification indicator interface.
/// An advanced ML-based indicator that uses Lorentzian distance for K-nearest neighbors classification
/// to predict market direction based on feature patterns.
/// </summary>
/// <remarks>
/// The Lorentzian Classification indicator:
/// - Uses k-NN with Lorentzian distance metric: L(x,y) = Σ log(1 + |xi - yi|)
/// - Extracts features: RSI, CCI, ADX, and price momentum components
/// - Stores historical patterns in a circular buffer for efficient lookback
/// - Provides Buy/Sell/Neutral signals with confidence scores
/// - Uses feature normalization for stable distance calculations
/// 
/// Available implementations:
/// - LorentzianClassification_FP: First-party implementation (optimized for streaming data)
/// 
/// Selection: Automatic based on performance profile, or set
/// ImplementationHint in parameters.
/// </remarks>
public interface ILorentzianClassification<TInput, TOutput> : IIndicator2
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// The number of nearest neighbors to consider (K in k-NN)
    /// </summary>
    int NeighborsCount { get; }
    
    /// <summary>
    /// The historical lookback period for pattern storage
    /// </summary>
    int LookbackPeriod { get; }
    
    /// <summary>
    /// The window size used for feature normalization
    /// </summary>
    int NormalizationWindow { get; }
    
    /// <summary>
    /// Current classification signal: 1.0 = Strong Buy, 0.0 = Neutral, -1.0 = Strong Sell
    /// </summary>
    TOutput Signal { get; }
    
    /// <summary>
    /// Confidence score for the current signal (0.0 to 1.0)
    /// Based on agreement among K nearest neighbors
    /// </summary>
    TOutput Confidence { get; }
    
    /// <summary>
    /// Current extracted features for debugging/analysis
    /// </summary>
    TOutput[] CurrentFeatures { get; }
    
    /// <summary>
    /// Gets the number of historical patterns currently stored
    /// </summary>
    int HistoricalPatternsCount { get; }
}

/// <summary>
/// Classification signal enumeration for cleaner API
/// </summary>
public enum ClassificationSignal
{
    /// <summary>
    /// Strong sell signal
    /// </summary>
    StrongSell = -2,
    
    /// <summary>
    /// Weak sell signal
    /// </summary>
    Sell = -1,
    
    /// <summary>
    /// Neutral signal (no clear direction)
    /// </summary>
    Neutral = 0,
    
    /// <summary>
    /// Weak buy signal
    /// </summary>
    Buy = 1,
    
    /// <summary>
    /// Strong buy signal
    /// </summary>
    StrongBuy = 2
}

/// <summary>
/// Historical pattern data structure for k-NN storage
/// </summary>
/// <typeparam name="TOutput">The numeric type for features and labels</typeparam>
public readonly struct HistoricalPattern<TOutput> where TOutput : struct, INumber<TOutput>
{
    /// <summary>
    /// Feature vector for this pattern
    /// </summary>
    public readonly TOutput[] Features { get; }
    
    /// <summary>
    /// The actual outcome/label for this pattern (-1, 0, or 1)
    /// </summary>
    public readonly TOutput Label { get; }
    
    /// <summary>
    /// The age of this pattern (for potential weighting)
    /// </summary>
    public readonly int Age { get; }
    
    /// <summary>
    /// Initializes a new historical pattern
    /// </summary>
    public HistoricalPattern(TOutput[] features, TOutput label, int age = 0)
    {
        Features = features ?? throw new ArgumentNullException(nameof(features));
        Label = label;
        Age = age;
    }
}
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Data;
using LionFire.Trading;
using System.Numerics;
using System.Reactive.Subjects;

namespace LionFire.Trading.Indicators.Base;

/// <summary>
/// Abstract base class for Lorentzian Classification implementations with common logic.
/// Provides the foundation for ML-based market classification using k-NN with Lorentzian distance.
/// </summary>
public abstract class LorentzianClassificationBase<TConcrete, TPrice, TOutput>
    : SingleInputIndicatorBase<TConcrete, PLorentzianClassification<TPrice, TOutput>, OHLC<TPrice>, TOutput>
    , ILorentzianClassification<OHLC<TPrice>, TOutput>
    where TConcrete : LorentzianClassificationBase<TConcrete, TPrice, TOutput>, IIndicator2<TConcrete, PLorentzianClassification<TPrice, TOutput>, OHLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Parameters

    protected readonly PLorentzianClassification<TPrice, TOutput> Parameters;

    /// <summary>
    /// The number of nearest neighbors to consider (K in k-NN)
    /// </summary>
    public int NeighborsCount => Parameters.NeighborsCount;

    /// <summary>
    /// The historical lookback period for pattern storage
    /// </summary>
    public int LookbackPeriod => Parameters.LookbackPeriod;

    /// <summary>
    /// The window size used for feature normalization
    /// </summary>
    public int NormalizationWindow => Parameters.NormalizationWindow;

    /// <summary>
    /// RSI period for feature extraction
    /// </summary>
    public int RSIPeriod => Parameters.RSIPeriod;

    /// <summary>
    /// CCI period for feature extraction
    /// </summary>
    public int CCIPeriod => Parameters.CCIPeriod;

    /// <summary>
    /// ADX period for feature extraction
    /// </summary>
    public int ADXPeriod => Parameters.ADXPeriod;

    /// <summary>
    /// Minimum confidence required to generate a signal
    /// </summary>
    public double MinConfidence => Parameters.MinConfidence;

    /// <summary>
    /// Number of bars ahead to look for classification labels
    /// </summary>
    public int LabelLookahead => Parameters.LabelLookahead;

    /// <summary>
    /// Threshold for labeling patterns as bullish/bearish
    /// </summary>
    public double LabelThreshold => Parameters.LabelThreshold;

    #region Derived

    /// <summary>
    /// Maximum lookback period required for the indicator
    /// </summary>
    public override int MaxLookback => Parameters.LookbackForInputSlot(PLorentzianClassification<TPrice, TOutput>.GetInputSlots().First());

    #endregion

    #endregion

    #region Lifecycle

    protected LorentzianClassificationBase(PLorentzianClassification<TPrice, TOutput> parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ValidateParameters();
    }

    #endregion

    #region Validation

    protected virtual void ValidateParameters()
    {
        Parameters.Validate();
    }

    #endregion

    #region State

    /// <summary>
    /// Current classification signal: 1.0 = Strong Buy, 0.0 = Neutral, -1.0 = Strong Sell
    /// </summary>
    public abstract TOutput Signal { get; }

    /// <summary>
    /// Confidence score for the current signal (0.0 to 1.0)
    /// Based on agreement among K nearest neighbors
    /// </summary>
    public abstract TOutput Confidence { get; }

    /// <summary>
    /// Current extracted features for debugging/analysis
    /// </summary>
    public abstract TOutput[] CurrentFeatures { get; }

    /// <summary>
    /// Gets the number of historical patterns currently stored
    /// </summary>
    public abstract int HistoricalPatternsCount { get; }

    /// <summary>
    /// Gets a value indicating whether the indicator has enough data to produce a value
    /// </summary>
    public abstract override bool IsReady { get; }

    protected Subject<IReadOnlyList<TOutput>>? subject;

    public static TOutput MissingOutputValue => TradingValueUtils<TOutput>.MissingValue;

    #endregion

    #region Static

    /// <summary>
    /// Gets the output slots for the Lorentzian Classification indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Confidence", 
                ValueType = typeof(TOutput),
            }
        ];

    /// <summary>
    /// Gets the output slots for the Lorentzian Classification indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PLorentzianClassification<TPrice, TOutput> p)
        => [
            new() {
                Name = "Signal",
                ValueType = typeof(TOutput),
            },
            new() {
                Name = "Confidence",
                ValueType = typeof(TOutput),
            }
        ];

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculates the Lorentzian distance between two feature vectors
    /// L(x,y) = Σ log(1 + |xi - yi|)
    /// </summary>
    protected static TOutput CalculateLorentzianDistance(TOutput[] features1, TOutput[] features2)
    {
        if (features1.Length != features2.Length)
            throw new ArgumentException("Feature vectors must have the same length");

        TOutput distance = TOutput.Zero;
        TOutput one = TOutput.One;

        for (int i = 0; i < features1.Length; i++)
        {
            TOutput diff = features1[i] - features2[i];
            // Take absolute value
            if (diff < TOutput.Zero)
                diff = -diff;
            
            // Calculate log(1 + |xi - yi|)
            TOutput logValue = TOutput.CreateChecked(Math.Log(1.0 + Convert.ToDouble(diff)));
            distance += logValue;
        }

        return distance;
    }

    /// <summary>
    /// Normalizes a feature vector using z-score normalization
    /// </summary>
    protected static void NormalizeFeatures(TOutput[] features, TOutput[] means, TOutput[] stdDevs)
    {
        for (int i = 0; i < features.Length; i++)
        {
            if (stdDevs[i] != TOutput.Zero)
            {
                features[i] = (features[i] - means[i]) / stdDevs[i];
            }
            else
            {
                features[i] = TOutput.Zero; // Handle case where std dev is zero
            }
        }
    }

    /// <summary>
    /// Calculates the mean of values in a circular buffer
    /// </summary>
    protected static TOutput CalculateMean(TOutput[] buffer, int count)
    {
        if (count == 0) return TOutput.Zero;

        TOutput sum = TOutput.Zero;
        for (int i = 0; i < count; i++)
        {
            sum += buffer[i];
        }
        return sum / TOutput.CreateChecked(count);
    }

    /// <summary>
    /// Calculates the standard deviation of values in a circular buffer
    /// </summary>
    protected static TOutput CalculateStandardDeviation(TOutput[] buffer, int count, TOutput mean)
    {
        if (count <= 1) return TOutput.One; // Return 1 to avoid division by zero

        TOutput sumOfSquares = TOutput.Zero;
        for (int i = 0; i < count; i++)
        {
            TOutput diff = buffer[i] - mean;
            sumOfSquares += diff * diff;
        }

        TOutput variance = sumOfSquares / TOutput.CreateChecked(count - 1);
        return TOutput.CreateChecked(Math.Sqrt(Convert.ToDouble(variance)));
    }

    /// <summary>
    /// Converts a price type to output type for calculations
    /// </summary>
    protected static TOutput ConvertToOutput(TPrice input)
    {
        // Handle common conversions efficiently
        if (typeof(TPrice) == typeof(TOutput))
        {
            return (TOutput)(object)input;
        }
        else
        {
            // Use generic conversion for other types
            return TOutput.CreateChecked(Convert.ToDouble(input));
        }
    }

    /// <summary>
    /// Determines the classification label based on future price movement
    /// </summary>
    protected TOutput CalculateLabel(TPrice currentClose, TPrice futureClose, double threshold)
    {
        double current = Convert.ToDouble(currentClose);
        double future = Convert.ToDouble(futureClose);
        
        double priceChange = (future - current) / current;
        
        if (priceChange > threshold)
            return TOutput.One; // Bullish
        else if (priceChange < -threshold)
            return -TOutput.One; // Bearish
        else
            return TOutput.Zero; // Neutral
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        base.Clear();
        subject?.OnCompleted();
        subject = null;
    }

    #endregion
}
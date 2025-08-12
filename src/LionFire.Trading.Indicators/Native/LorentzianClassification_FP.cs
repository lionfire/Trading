using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of the Lorentzian Classification indicator.
/// Uses k-NN with Lorentzian distance metric for market direction prediction.
/// Optimized for streaming data with efficient circular buffers and feature extraction.
/// 
/// Features extracted:
/// - RSI (Relative Strength Index)
/// - CCI (Commodity Channel Index) changes
/// - ADX (Average Directional Index)  
/// - Price momentum components
/// 
/// The algorithm:
/// 1. Extract features from current OHLC bar
/// 2. Normalize features using rolling statistics
/// 3. Find K nearest historical patterns using Lorentzian distance
/// 4. Vote based on historical outcomes to predict direction
/// 5. Calculate confidence based on neighbor agreement
/// </summary>
public class LorentzianClassification_FP<TPrice, TOutput> 
    : LorentzianClassificationBase<LorentzianClassification_FP<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<LorentzianClassification_FP<TPrice, TOutput>, PLorentzianClassification<TPrice, TOutput>, OHLC<TPrice>, TOutput>
    , ILorentzianClassification<OHLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Constants

    private const int FEATURE_COUNT = 6; // RSI, CCI_change, ADX, returns, volatility, momentum

    #endregion

    #region Static

    /// <summary>
    /// Creates a new LorentzianClassification indicator instance
    /// </summary>
    public static LorentzianClassification_FP<TPrice, TOutput> Create(PLorentzianClassification<TPrice, TOutput> p) 
        => new LorentzianClassification_FP<TPrice, TOutput>(p);

    #endregion

    #region Fields

    // Historical pattern storage (circular buffer)
    private readonly HistoricalPattern<TOutput>[] historicalPatterns;
    private int historicalIndex = 0;
    private int historicalCount = 0;

    // OHLC price history for feature calculation
    private readonly OHLC<TPrice>[] priceHistory;
    private int priceIndex = 0;
    private int priceCount = 0;

    // Feature normalization rolling windows
    private readonly TOutput[][] featureBuffers; // [feature][time]
    private readonly int[] featureBufferIndices;
    private readonly int[] featureBufferCounts;
    
    // Feature statistics for normalization
    private readonly TOutput[] featureMeans;
    private readonly TOutput[] featureStdDevs;
    
    // RSI calculation state
    private TOutput rsiAvgGain = TOutput.Zero;
    private TOutput rsiAvgLoss = TOutput.Zero;
    private TOutput rsiPreviousClose;
    private int rsiDataPoints = 0;
    private bool rsiHasPreviousClose = false;
    
    // CCI calculation state  
    private readonly TOutput[] typicalPrices;
    private int typicalPriceIndex = 0;
    private int typicalPriceCount = 0;
    private TOutput typicalPriceSum = TOutput.Zero;
    private TOutput previousCCI = TOutput.Zero;
    
    // Current state
    private TOutput currentSignal = TOutput.Zero;
    private TOutput currentConfidence = TOutput.Zero;
    private readonly TOutput[] currentFeatures;
    
    // Future price tracking for labeling  
    private readonly TPrice[] futurePrices;
    private int futurePriceIndex = 0;
    private int futurePriceCount = 0;

    #endregion

    #region Properties

    /// <summary>
    /// Current classification signal
    /// </summary>
    public override TOutput Signal => currentSignal;

    /// <summary>
    /// Current confidence score
    /// </summary>
    public override TOutput Confidence => currentConfidence;

    /// <summary>
    /// Current extracted features
    /// </summary>
    public override TOutput[] CurrentFeatures => (TOutput[])currentFeatures.Clone();

    /// <summary>
    /// Number of historical patterns stored
    /// </summary>
    public override int HistoricalPatternsCount => historicalCount;

    /// <summary>
    /// Whether the indicator has enough data
    /// </summary>
    public override bool IsReady => historicalCount >= NeighborsCount && 
                                    priceCount >= Math.Max(Math.Max(RSIPeriod, CCIPeriod), ADXPeriod);

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of the LorentzianClassification indicator
    /// </summary>
    public LorentzianClassification_FP(PLorentzianClassification<TPrice, TOutput> parameters)
        : base(parameters)
    {
        // Initialize historical pattern storage
        historicalPatterns = new HistoricalPattern<TOutput>[LookbackPeriod];
        
        // Initialize price history
        var maxPeriod = Math.Max(Math.Max(RSIPeriod, CCIPeriod), ADXPeriod);
        priceHistory = new OHLC<TPrice>[maxPeriod + LabelLookahead + 10]; // Extra buffer
        
        // Initialize feature normalization buffers
        featureBuffers = new TOutput[FEATURE_COUNT][];
        featureBufferIndices = new int[FEATURE_COUNT];
        featureBufferCounts = new int[FEATURE_COUNT];
        for (int i = 0; i < FEATURE_COUNT; i++)
        {
            featureBuffers[i] = new TOutput[NormalizationWindow];
        }
        
        // Initialize feature statistics
        featureMeans = new TOutput[FEATURE_COUNT];
        featureStdDevs = new TOutput[FEATURE_COUNT];
        
        // Initialize CCI state
        typicalPrices = new TOutput[CCIPeriod];
        
        // Initialize current features
        currentFeatures = new TOutput[FEATURE_COUNT];
        
        // Initialize future price tracking
        futurePrices = new TPrice[LabelLookahead + 5]; // Extra buffer
        
        // Initialize RSI state
        rsiPreviousClose = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    /// <summary>
    /// Process a batch of OHLC inputs
    /// </summary>
    public override void OnBarBatch(IReadOnlyList<OHLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Store price in history buffer
            priceHistory[priceIndex] = input;
            priceIndex = (priceIndex + 1) % priceHistory.Length;
            if (priceCount < priceHistory.Length)
                priceCount++;

            // Track future prices for labeling
            futurePrices[futurePriceIndex] = input.Close;
            futurePriceIndex = (futurePriceIndex + 1) % futurePrices.Length;
            if (futurePriceCount < futurePrices.Length)
                futurePriceCount++;

            // Extract features from current bar
            if (priceCount >= Math.Max(Math.Max(RSIPeriod, CCIPeriod), ADXPeriod))
            {
                ExtractFeatures(input);
                
                // Normalize features using rolling statistics
                UpdateFeatureStatistics();
                var normalizedFeatures = (TOutput[])currentFeatures.Clone();
                NormalizeFeatures(normalizedFeatures, featureMeans, featureStdDevs);
                
                // Create and store historical pattern if we have enough history
                if (CanCreatePattern())
                {
                    var label = CalculateLabelFromHistory();
                    var pattern = new HistoricalPattern<TOutput>(normalizedFeatures, label);
                    StorePattern(pattern);
                }
                
                // Perform classification if we have enough patterns
                if (IsReady)
                {
                    var (signal, confidence) = ClassifyCurrentPattern(normalizedFeatures);
                    currentSignal = signal;
                    currentConfidence = confidence;
                }
            }

            // Output current signal and confidence 
            TOutput outputSignal = IsReady ? currentSignal : MissingOutputValue;
            TOutput outputConfidence = IsReady ? currentConfidence : MissingOutputValue;
            
            // For this implementation, we'll output the signal as the primary value
            PopulateOutput(outputSignal, output, ref outputIndex, ref outputSkip);
            
            // Notify observers if any
            if (subject != null)
            {
                subject.OnNext([outputSignal, outputConfidence]);
            }
        }
    }

    #endregion

    #region Feature Extraction

    /// <summary>
    /// Extracts all features from the current OHLC bar
    /// </summary>
    private void ExtractFeatures(OHLC<TPrice> currentBar)
    {
        // Feature 0: RSI
        currentFeatures[0] = CalculateRSI(currentBar.Close);
        
        // Feature 1: CCI Change
        var currentCCI = CalculateCCI();
        currentFeatures[1] = currentCCI - previousCCI;
        previousCCI = currentCCI;
        
        // Feature 2: ADX (simplified momentum-based approximation)
        currentFeatures[2] = CalculateADXApproximation();
        
        // Feature 3: Price Returns (Close-to-Close)
        currentFeatures[3] = CalculateReturns(currentBar.Close);
        
        // Feature 4: Volatility (High-Low range normalized by close)
        currentFeatures[4] = CalculateVolatility(currentBar);
        
        // Feature 5: Price Momentum (rate of change)
        currentFeatures[5] = CalculateMomentum(currentBar.Close);
    }

    /// <summary>
    /// Calculate RSI using Wilder's smoothing method
    /// </summary>
    private TOutput CalculateRSI(TPrice close)
    {
        var currentClose = ConvertToOutput(close);
        
        if (!rsiHasPreviousClose)
        {
            rsiPreviousClose = currentClose;
            rsiHasPreviousClose = true;
            return TOutput.CreateChecked(50); // Neutral RSI
        }

        var change = currentClose - rsiPreviousClose;
        var gain = change > TOutput.Zero ? change : TOutput.Zero;
        var loss = change < TOutput.Zero ? -change : TOutput.Zero;

        if (rsiDataPoints < RSIPeriod)
        {
            // Initial accumulation period
            rsiAvgGain = (rsiAvgGain * TOutput.CreateChecked(rsiDataPoints) + gain) / TOutput.CreateChecked(rsiDataPoints + 1);
            rsiAvgLoss = (rsiAvgLoss * TOutput.CreateChecked(rsiDataPoints) + loss) / TOutput.CreateChecked(rsiDataPoints + 1);
            rsiDataPoints++;
        }
        else
        {
            // Wilder's smoothing
            var alpha = TOutput.One / TOutput.CreateChecked(RSIPeriod);
            rsiAvgGain = (TOutput.One - alpha) * rsiAvgGain + alpha * gain;
            rsiAvgLoss = (TOutput.One - alpha) * rsiAvgLoss + alpha * loss;
        }

        rsiPreviousClose = currentClose;

        if (rsiAvgLoss == TOutput.Zero)
            return TOutput.CreateChecked(100);

        var rs = rsiAvgGain / rsiAvgLoss;
        var hundred = TOutput.CreateChecked(100);
        return hundred - (hundred / (TOutput.One + rs));
    }

    /// <summary>
    /// Calculate CCI (Commodity Channel Index)
    /// </summary>
    private TOutput CalculateCCI()
    {
        if (priceCount < CCIPeriod) return TOutput.Zero;

        // Calculate typical price: (H + L + C) / 3
        var currentBar = priceHistory[(priceIndex - 1 + priceHistory.Length) % priceHistory.Length];
        var typicalPrice = (ConvertToOutput(currentBar.High) + ConvertToOutput(currentBar.Low) + ConvertToOutput(currentBar.Close)) / TOutput.CreateChecked(3);

        // Update circular buffer
        if (typicalPriceCount >= CCIPeriod)
        {
            typicalPriceSum -= typicalPrices[typicalPriceIndex];
        }
        
        typicalPrices[typicalPriceIndex] = typicalPrice;
        typicalPriceSum += typicalPrice;
        typicalPriceIndex = (typicalPriceIndex + 1) % CCIPeriod;
        
        if (typicalPriceCount < CCIPeriod)
            typicalPriceCount++;

        if (typicalPriceCount < CCIPeriod) return TOutput.Zero;

        // Calculate SMA of typical prices
        var smaTypicalPrice = typicalPriceSum / TOutput.CreateChecked(CCIPeriod);

        // Calculate mean deviation
        TOutput deviationSum = TOutput.Zero;
        for (int i = 0; i < CCIPeriod; i++)
        {
            var deviation = typicalPrices[i] - smaTypicalPrice;
            if (deviation < TOutput.Zero) deviation = -deviation;
            deviationSum += deviation;
        }
        
        var meanDeviation = deviationSum / TOutput.CreateChecked(CCIPeriod);
        
        if (meanDeviation == TOutput.Zero) return TOutput.Zero;
        
        // CCI = (Current TP - SMA(TP)) / (0.015 * Mean Deviation)
        var constant = TOutput.CreateChecked(0.015);
        return (typicalPrice - smaTypicalPrice) / (constant * meanDeviation);
    }

    /// <summary>
    /// Calculate a simplified ADX approximation using price momentum
    /// </summary>
    private TOutput CalculateADXApproximation()
    {
        if (priceCount < ADXPeriod) return TOutput.Zero;

        TOutput positiveMovement = TOutput.Zero;
        TOutput negativeMovement = TOutput.Zero;
        TOutput trueRangeSum = TOutput.Zero;

        // Look back over ADX period
        for (int i = 1; i < Math.Min(ADXPeriod, priceCount); i++)
        {
            var current = priceHistory[(priceIndex - i + priceHistory.Length) % priceHistory.Length];
            var previous = priceHistory[(priceIndex - i - 1 + priceHistory.Length) % priceHistory.Length];

            var currentHigh = ConvertToOutput(current.High);
            var currentLow = ConvertToOutput(current.Low);
            var currentClose = ConvertToOutput(current.Close);
            var previousHigh = ConvertToOutput(previous.High);
            var previousLow = ConvertToOutput(previous.Low);
            var previousClose = ConvertToOutput(previous.Close);

            // Directional movement
            var upMove = currentHigh - previousHigh;
            var downMove = previousLow - currentLow;

            if (upMove > downMove && upMove > TOutput.Zero)
                positiveMovement += upMove;
            if (downMove > upMove && downMove > TOutput.Zero)  
                negativeMovement += downMove;

            // True Range
            var tr1 = currentHigh - currentLow;
            var tr2 = currentHigh - previousClose;
            if (tr2 < TOutput.Zero) tr2 = -tr2;
            var tr3 = currentLow - previousClose;
            if (tr3 < TOutput.Zero) tr3 = -tr3;
            
            var trueRange = tr1;
            if (tr2 > trueRange) trueRange = tr2;
            if (tr3 > trueRange) trueRange = tr3;
            
            trueRangeSum += trueRange;
        }

        if (trueRangeSum == TOutput.Zero) return TOutput.Zero;

        var plusDI = (positiveMovement / trueRangeSum) * TOutput.CreateChecked(100);
        var minusDI = (negativeMovement / trueRangeSum) * TOutput.CreateChecked(100);
        
        var sum = plusDI + minusDI;
        if (sum == TOutput.Zero) return TOutput.Zero;
        
        var diff = plusDI - minusDI;
        if (diff < TOutput.Zero) diff = -diff;
        
        return (diff / sum) * TOutput.CreateChecked(100);
    }

    /// <summary>
    /// Calculate price returns (percentage change)
    /// </summary>
    private TOutput CalculateReturns(TPrice close)
    {
        if (priceCount < 2) return TOutput.Zero;
        
        var currentClose = ConvertToOutput(close);
        var previousBar = priceHistory[(priceIndex - 2 + priceHistory.Length) % priceHistory.Length];
        var previousClose = ConvertToOutput(previousBar.Close);
        
        if (previousClose == TOutput.Zero) return TOutput.Zero;
        
        return (currentClose - previousClose) / previousClose;
    }

    /// <summary>
    /// Calculate volatility as normalized high-low range
    /// </summary>
    private TOutput CalculateVolatility(OHLC<TPrice> bar)
    {
        var high = ConvertToOutput(bar.High);
        var low = ConvertToOutput(bar.Low);
        var close = ConvertToOutput(bar.Close);
        
        if (close == TOutput.Zero) return TOutput.Zero;
        
        return (high - low) / close;
    }

    /// <summary>
    /// Calculate momentum as rate of change over a short period
    /// </summary>
    private TOutput CalculateMomentum(TPrice close)
    {
        int momentumPeriod = Math.Min(5, priceCount - 1);
        if (priceCount <= momentumPeriod) return TOutput.Zero;
        
        var currentClose = ConvertToOutput(close);
        var pastBar = priceHistory[(priceIndex - momentumPeriod - 1 + priceHistory.Length) % priceHistory.Length];
        var pastClose = ConvertToOutput(pastBar.Close);
        
        if (pastClose == TOutput.Zero) return TOutput.Zero;
        
        return (currentClose - pastClose) / pastClose;
    }

    #endregion

    #region Feature Normalization

    /// <summary>
    /// Update rolling statistics for feature normalization
    /// </summary>
    private void UpdateFeatureStatistics()
    {
        for (int f = 0; f < FEATURE_COUNT; f++)
        {
            // Add current feature to rolling buffer
            featureBuffers[f][featureBufferIndices[f]] = currentFeatures[f];
            featureBufferIndices[f] = (featureBufferIndices[f] + 1) % NormalizationWindow;
            
            if (featureBufferCounts[f] < NormalizationWindow)
                featureBufferCounts[f]++;
            
            // Calculate rolling mean and std deviation
            featureMeans[f] = CalculateMean(featureBuffers[f], featureBufferCounts[f]);
            featureStdDevs[f] = CalculateStandardDeviation(featureBuffers[f], featureBufferCounts[f], featureMeans[f]);
        }
    }

    #endregion

    #region Pattern Management

    /// <summary>
    /// Check if we can create a labeled pattern
    /// </summary>
    private bool CanCreatePattern()
    {
        return futurePriceCount > LabelLookahead && priceCount > LabelLookahead;
    }

    /// <summary>
    /// Calculate label based on future price movement
    /// </summary>
    private TOutput CalculateLabelFromHistory()
    {
        if (futurePriceCount <= LabelLookahead) return TOutput.Zero;
        
        // Get current close price
        var currentBar = priceHistory[(priceIndex - 1 + priceHistory.Length) % priceHistory.Length];
        var currentClose = currentBar.Close;
        
        // Get future close price
        var futureClose = futurePrices[(futurePriceIndex - LabelLookahead + futurePrices.Length) % futurePrices.Length];
        
        return CalculateLabel(currentClose, futureClose, LabelThreshold);
    }

    /// <summary>
    /// Store a pattern in the historical buffer
    /// </summary>
    private void StorePattern(HistoricalPattern<TOutput> pattern)
    {
        historicalPatterns[historicalIndex] = pattern;
        historicalIndex = (historicalIndex + 1) % LookbackPeriod;
        
        if (historicalCount < LookbackPeriod)
            historicalCount++;
    }

    #endregion

    #region Classification

    /// <summary>
    /// Classify the current pattern using k-NN with Lorentzian distance
    /// </summary>
    private (TOutput signal, TOutput confidence) ClassifyCurrentPattern(TOutput[] features)
    {
        if (historicalCount < NeighborsCount)
            return (TOutput.Zero, TOutput.Zero);

        // Calculate distances to all historical patterns
        var distances = new (TOutput distance, TOutput label, int index)[historicalCount];
        
        for (int i = 0; i < historicalCount; i++)
        {
            var pattern = historicalPatterns[i];
            var distance = CalculateLorentzianDistance(features, pattern.Features);
            distances[i] = (distance, pattern.Label, i);
        }

        // Sort by distance and take K nearest neighbors
        Array.Sort(distances, (a, b) => a.distance.CompareTo(b.distance));
        
        // Vote among K nearest neighbors
        TOutput bullishVotes = TOutput.Zero;
        TOutput bearishVotes = TOutput.Zero;
        TOutput neutralVotes = TOutput.Zero;
        
        int actualNeighbors = Math.Min(NeighborsCount, historicalCount);
        
        for (int i = 0; i < actualNeighbors; i++)
        {
            var label = distances[i].label;
            if (label > TOutput.Zero)
                bullishVotes += TOutput.One;
            else if (label < TOutput.Zero)
                bearishVotes += TOutput.One;
            else
                neutralVotes += TOutput.One;
        }

        // Determine signal based on majority vote
        TOutput signal = TOutput.Zero;
        TOutput maxVotes = neutralVotes;
        
        if (bullishVotes > maxVotes)
        {
            signal = TOutput.One;
            maxVotes = bullishVotes;
        }
        if (bearishVotes > maxVotes)
        {
            signal = -TOutput.One;
            maxVotes = bearishVotes;
        }

        // Calculate confidence as percentage of neighbors agreeing
        TOutput confidence = maxVotes / TOutput.CreateChecked(actualNeighbors);
        
        // Apply minimum confidence threshold
        if (Convert.ToDouble(confidence) < MinConfidence)
        {
            signal = TOutput.Zero; // Not confident enough
            confidence = TOutput.Zero;
        }

        return (signal, confidence);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to populate the output buffer
    /// </summary>
    private static void PopulateOutput(TOutput value, TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        if (outputSkip > 0) 
        { 
            outputSkip--; 
        }
        else if (outputBuffer != null && outputIndex < outputBuffer.Length) 
        {
            outputBuffer[outputIndex++] = value;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clears and resets the indicator state
    /// </summary>
    public override void Clear() 
    { 
        base.Clear();
        
        // Reset all buffers and counters
        historicalIndex = 0;
        historicalCount = 0;
        priceIndex = 0;
        priceCount = 0;
        
        Array.Clear(featureBufferIndices, 0, featureBufferIndices.Length);
        Array.Clear(featureBufferCounts, 0, featureBufferCounts.Length);
        
        for (int i = 0; i < FEATURE_COUNT; i++)
        {
            Array.Clear(featureBuffers[i], 0, featureBuffers[i].Length);
        }
        
        Array.Clear(featureMeans, 0, featureMeans.Length);
        Array.Clear(featureStdDevs, 0, featureStdDevs.Length);
        
        // Reset RSI state
        rsiAvgGain = TOutput.Zero;
        rsiAvgLoss = TOutput.Zero;
        rsiPreviousClose = TOutput.Zero;
        rsiDataPoints = 0;
        rsiHasPreviousClose = false;
        
        // Reset CCI state
        typicalPriceIndex = 0;
        typicalPriceCount = 0;
        typicalPriceSum = TOutput.Zero;
        previousCCI = TOutput.Zero;
        
        // Reset current state
        currentSignal = TOutput.Zero;
        currentConfidence = TOutput.Zero;
        Array.Clear(currentFeatures, 0, currentFeatures.Length);
        
        // Reset future price tracking
        futurePriceIndex = 0;
        futurePriceCount = 0;
    }

    #endregion
}
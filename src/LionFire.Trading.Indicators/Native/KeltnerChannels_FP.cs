using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using LionFire.Trading;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Keltner Channels using EMA and ATR calculations
/// Optimized for streaming updates
/// </summary>
public class KeltnerChannels_FP<TInput, TOutput>
    : KeltnerChannelsBase<TInput, TOutput>
    , IIndicator2<KeltnerChannels_FP<TInput, TOutput>, PKeltnerChannels<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly Queue<TOutput> priceWindow;
    private readonly Queue<TOutput> trueRangeWindow;
    
    // EMA calculation fields
    private TOutput emaValue;
    private TOutput emaMultiplier;
    private bool emaInitialized = false;
    
    // ATR calculation fields
    private TOutput atrSum;
    private TOutput currentAtr;
    
    // Previous values for True Range calculation
    private TOutput previousClose;
    private bool hasPreviousClose = false;
    
    // Current output values
    private TOutput currentUpperBand;
    private TOutput currentMiddleLine;
    private TOutput currentLowerBand;
    
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput UpperBand => currentUpperBand;
    
    public override TOutput MiddleLine => currentMiddleLine;
    
    public override TOutput LowerBand => currentLowerBand;
    
    public override TOutput AtrValue => currentAtr;
    
    public override bool IsReady => dataPointsReceived >= Math.Max(Parameters.Period, Parameters.AtrPeriod);

    #endregion

    #region Lifecycle

    public static KeltnerChannels_FP<TInput, TOutput> Create(PKeltnerChannels<TInput, TOutput> p)
        => new KeltnerChannels_FP<TInput, TOutput>(p);

    public KeltnerChannels_FP(PKeltnerChannels<TInput, TOutput> parameters) : base(parameters)
    {
        var maxPeriod = Math.Max(Parameters.Period, Parameters.AtrPeriod);
        priceWindow = new Queue<TOutput>(Parameters.Period);
        trueRangeWindow = new Queue<TOutput>(Parameters.AtrPeriod);
        
        // Calculate EMA multiplier: 2 / (Period + 1)
        var two = TOutput.CreateChecked(2);
        var periodPlusOne = TOutput.CreateChecked(Parameters.Period + 1);
        emaMultiplier = two / periodPlusOne;
        
        atrSum = TOutput.Zero;
        currentAtr = MissingOutputValue;
        emaValue = MissingOutputValue;
        currentUpperBand = MissingOutputValue;
        currentMiddleLine = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
        previousClose = MissingOutputValue;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            ProcessInput(input);
            
            // Output the channel values
            var upper = IsReady ? currentUpperBand : MissingOutputValue;
            var middle = IsReady ? currentMiddleLine : MissingOutputValue;
            var lower = IsReady ? currentLowerBand : MissingOutputValue;
            
            // Output in the order: Upper, Middle, Lower
            if (outputSkip > 0)
            {
                outputSkip--;
                if (outputSkip > 0) outputSkip--;
                if (outputSkip > 0) outputSkip--;
            }
            else if (output != null)
            {
                if (outputIndex < output.Length) output[outputIndex++] = upper;
                if (outputIndex < output.Length) output[outputIndex++] = middle;
                if (outputIndex < output.Length) output[outputIndex++] = lower;
            }
        }
        
        // Notify observers if any
        if (subject != null && output != null && outputIndex > 0)
        {
            var results = new TOutput[outputIndex];
            Array.Copy(output, results, outputIndex);
            subject.OnNext(results);
        }
    }

    private void ProcessInput(TInput input)
    {
        // For this implementation, we assume TInput is a single price (Close)
        // For a more complete implementation, TInput would be HLC<T>
        var close = TOutput.CreateChecked(Convert.ToDecimal(input));
        
        // Calculate True Range if we have previous close
        if (hasPreviousClose)
        {
            // True Range = max(H-L, |H-PC|, |L-PC|)
            // Since we only have Close, TR = |Close - PreviousClose|
            var trueRange = close > previousClose ? close - previousClose : previousClose - close;
            
            UpdateAtr(trueRange);
        }
        
        // Update EMA (Middle Line)
        UpdateEma(close);
        
        // Calculate bands if both EMA and ATR are ready
        if (emaInitialized && trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            var bandDistance = currentAtr * Parameters.AtrMultiplier;
            currentUpperBand = emaValue + bandDistance;
            currentMiddleLine = emaValue;
            currentLowerBand = emaValue - bandDistance;
        }
        
        previousClose = close;
        hasPreviousClose = true;
        dataPointsReceived++;
    }

    private void UpdateEma(TOutput price)
    {
        if (!emaInitialized)
        {
            // Initialize EMA with first price
            emaValue = price;
            emaInitialized = true;
            currentMiddleLine = emaValue;
        }
        else
        {
            // EMA = (Price * Multiplier) + (PreviousEMA * (1 - Multiplier))
            var one = TOutput.CreateChecked(1);
            emaValue = (price * emaMultiplier) + (emaValue * (one - emaMultiplier));
            currentMiddleLine = emaValue;
        }
    }

    private void UpdateAtr(TOutput trueRange)
    {
        // Update the sliding window for ATR
        if (trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            // Remove oldest true range from sum
            atrSum -= trueRangeWindow.Dequeue();
        }
        
        // Add new true range
        trueRangeWindow.Enqueue(trueRange);
        atrSum += trueRange;
        
        // Calculate ATR as Simple Moving Average of True Range
        if (trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            var atrPeriod = TOutput.CreateChecked(Parameters.AtrPeriod);
            currentAtr = atrSum / atrPeriod;
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        priceWindow.Clear();
        trueRangeWindow.Clear();
        
        emaValue = MissingOutputValue;
        emaInitialized = false;
        atrSum = TOutput.Zero;
        currentAtr = MissingOutputValue;
        
        currentUpperBand = MissingOutputValue;
        currentMiddleLine = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
        
        previousClose = MissingOutputValue;
        hasPreviousClose = false;
        dataPointsReceived = 0;
    }

    #endregion
}

/// <summary>
/// Specialized version for HLC input data
/// </summary>
public class KeltnerChannelsHLC_FP<TPriceInput, TOutput>
    : KeltnerChannelsBase<HLC<TPriceInput>, TOutput>
    , IIndicator2<KeltnerChannelsHLC_FP<TPriceInput, TOutput>, PKeltnerChannels<HLC<TPriceInput>, TOutput>, HLC<TPriceInput>, TOutput>
    where TPriceInput : struct, INumber<TPriceInput>
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly Queue<TOutput> trueRangeWindow;
    
    // EMA calculation fields
    private TOutput emaValue;
    private TOutput emaMultiplier;
    private bool emaInitialized = false;
    
    // ATR calculation fields
    private TOutput atrSum;
    private TOutput currentAtr;
    
    // Previous close for True Range calculation
    private TOutput previousClose;
    private bool hasPreviousClose = false;
    
    // Current output values
    private TOutput currentUpperBand;
    private TOutput currentMiddleLine;
    private TOutput currentLowerBand;
    
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput UpperBand => currentUpperBand;
    
    public override TOutput MiddleLine => currentMiddleLine;
    
    public override TOutput LowerBand => currentLowerBand;
    
    public override TOutput AtrValue => currentAtr;
    
    public override bool IsReady => dataPointsReceived >= Math.Max(Parameters.Period, Parameters.AtrPeriod);

    #endregion

    #region Lifecycle

    public static KeltnerChannelsHLC_FP<TPriceInput, TOutput> Create(PKeltnerChannels<HLC<TPriceInput>, TOutput> p)
        => new KeltnerChannelsHLC_FP<TPriceInput, TOutput>(p);

    public KeltnerChannelsHLC_FP(PKeltnerChannels<HLC<TPriceInput>, TOutput> parameters) : base(parameters)
    {
        trueRangeWindow = new Queue<TOutput>(Parameters.AtrPeriod);
        
        // Calculate EMA multiplier: 2 / (Period + 1)
        var two = TOutput.CreateChecked(2);
        var periodPlusOne = TOutput.CreateChecked(Parameters.Period + 1);
        emaMultiplier = two / periodPlusOne;
        
        atrSum = TOutput.Zero;
        currentAtr = MissingOutputValue;
        emaValue = MissingOutputValue;
        currentUpperBand = MissingOutputValue;
        currentMiddleLine = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
        previousClose = MissingOutputValue;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPriceInput>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            ProcessHLCInput(input);
            
            // Output the channel values
            var upper = IsReady ? currentUpperBand : MissingOutputValue;
            var middle = IsReady ? currentMiddleLine : MissingOutputValue;
            var lower = IsReady ? currentLowerBand : MissingOutputValue;
            
            // Output in the order: Upper, Middle, Lower
            if (outputSkip > 0)
            {
                outputSkip--;
                if (outputSkip > 0) outputSkip--;
                if (outputSkip > 0) outputSkip--;
            }
            else if (output != null)
            {
                if (outputIndex < output.Length) output[outputIndex++] = upper;
                if (outputIndex < output.Length) output[outputIndex++] = middle;
                if (outputIndex < output.Length) output[outputIndex++] = lower;
            }
        }
        
        // Notify observers if any
        if (subject != null && output != null && outputIndex > 0)
        {
            var results = new TOutput[outputIndex];
            Array.Copy(output, results, outputIndex);
            subject.OnNext(results);
        }
    }

    private void ProcessHLCInput(HLC<TPriceInput> hlc)
    {
        var high = TOutput.CreateChecked(Convert.ToDecimal(hlc.High));
        var low = TOutput.CreateChecked(Convert.ToDecimal(hlc.Low));
        var close = TOutput.CreateChecked(Convert.ToDecimal(hlc.Close));
        
        // Calculate True Range: max(H-L, |H-PC|, |L-PC|)
        if (hasPreviousClose)
        {
            var hlDiff = high - low;
            var hpcDiff = high > previousClose ? high - previousClose : previousClose - high;
            var lpcDiff = low > previousClose ? low - previousClose : previousClose - low;
            
            var trueRange = hlDiff;
            if (hpcDiff > trueRange) trueRange = hpcDiff;
            if (lpcDiff > trueRange) trueRange = lpcDiff;
            
            UpdateAtr(trueRange);
        }
        
        // Update EMA with close price
        UpdateEma(close);
        
        // Calculate bands if both EMA and ATR are ready
        if (emaInitialized && trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            var bandDistance = currentAtr * Parameters.AtrMultiplier;
            currentUpperBand = emaValue + bandDistance;
            currentMiddleLine = emaValue;
            currentLowerBand = emaValue - bandDistance;
        }
        
        previousClose = close;
        hasPreviousClose = true;
        dataPointsReceived++;
    }

    private void UpdateEma(TOutput price)
    {
        if (!emaInitialized)
        {
            // Initialize EMA with first price
            emaValue = price;
            emaInitialized = true;
            currentMiddleLine = emaValue;
        }
        else
        {
            // EMA = (Price * Multiplier) + (PreviousEMA * (1 - Multiplier))
            var one = TOutput.CreateChecked(1);
            emaValue = (price * emaMultiplier) + (emaValue * (one - emaMultiplier));
            currentMiddleLine = emaValue;
        }
    }

    private void UpdateAtr(TOutput trueRange)
    {
        // Update the sliding window for ATR
        if (trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            // Remove oldest true range from sum
            atrSum -= trueRangeWindow.Dequeue();
        }
        
        // Add new true range
        trueRangeWindow.Enqueue(trueRange);
        atrSum += trueRange;
        
        // Calculate ATR as Simple Moving Average of True Range
        if (trueRangeWindow.Count >= Parameters.AtrPeriod)
        {
            var atrPeriod = TOutput.CreateChecked(Parameters.AtrPeriod);
            currentAtr = atrSum / atrPeriod;
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        trueRangeWindow.Clear();
        
        emaValue = MissingOutputValue;
        emaInitialized = false;
        atrSum = TOutput.Zero;
        currentAtr = MissingOutputValue;
        
        currentUpperBand = MissingOutputValue;
        currentMiddleLine = MissingOutputValue;
        currentLowerBand = MissingOutputValue;
        
        previousClose = MissingOutputValue;
        hasPreviousClose = false;
        dataPointsReceived = 0;
    }

    #endregion
}
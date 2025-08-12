using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators;
using LionFire.Trading;
using System.Numerics;
using System.Linq;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of ZigZag indicator
/// Identifies significant price swing highs and lows by filtering out minor price movements
/// </summary>
public class ZigZag_FP<TPrice, TOutput>
    : ZigZagBase<HLC<TPrice>, TOutput>
    , IIndicator2<ZigZag_FP<TPrice, TOutput>, PZigZag<HLC<TPrice>, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly List<ZigZagPivot<TOutput>> pivotHistory;
    private readonly List<PotentialPivot> potentialPivots;
    
    private TOutput currentZigZagValue;
    private TOutput lastPivotHigh;
    private TOutput lastPivotLow;
    private int currentDirection; // 1 for up, -1 for down, 0 for indeterminate
    
    private int barIndex = 0;
    private bool isReady = false;
    
    // Track the current trend to identify potential reversals
    private TrendState currentTrend = TrendState.Unknown;
    private TOutput trendHigh;
    private TOutput trendLow;
    private int trendHighBarIndex;
    private int trendLowBarIndex;

    #endregion

    #region Properties

    public override TOutput CurrentValue => isReady ? currentZigZagValue : MissingOutputValue;
    
    public override TOutput LastPivotHigh => lastPivotHigh;
    
    public override TOutput LastPivotLow => lastPivotLow;
    
    public override int Direction => currentDirection;
    
    public override bool IsReady => isReady;
    
    public override IReadOnlyList<ZigZagPivot<TOutput>>? RecentPivots => pivotHistory.AsReadOnly();

    #endregion

    #region Lifecycle

    public static ZigZag_FP<TPrice, TOutput> Create(PZigZag<HLC<TPrice>, TOutput> p)
        => new ZigZag_FP<TPrice, TOutput>(p);

    public ZigZag_FP(PZigZag<HLC<TPrice>, TOutput> parameters) : base(parameters)
    {
        pivotHistory = new List<ZigZagPivot<TOutput>>();
        potentialPivots = new List<PotentialPivot>();
        
        currentZigZagValue = MissingOutputValue;
        lastPivotHigh = MissingOutputValue;
        lastPivotLow = MissingOutputValue;
        currentDirection = 0;
        
        trendHigh = TOutput.CreateChecked(decimal.MinValue);
        trendLow = TOutput.CreateChecked(decimal.MaxValue);
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type
            var high = TOutput.CreateChecked(Convert.ToDecimal(input.High));
            var low = TOutput.CreateChecked(Convert.ToDecimal(input.Low));
            var close = TOutput.CreateChecked(Convert.ToDecimal(input.Close));
            
            ProcessBar(high, low, close, barIndex);
            barIndex++;
            
            var outputValue = IsReady ? currentZigZagValue : MissingOutputValue;
            
            if (outputSkip > 0)
            {
                outputSkip--;
            }
            else if (output != null && outputIndex < output.Length)
            {
                output[outputIndex++] = outputValue;
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

    #endregion

    #region ZigZag Algorithm

    private void ProcessBar(TOutput high, TOutput low, TOutput close, int currentBarIndex)
    {
        // Update trend tracking
        UpdateTrendTracking(high, low, currentBarIndex);
        
        // Check for potential pivot points
        CheckForPivots(high, low, currentBarIndex);
        
        // Clean up old potential pivots that haven't been confirmed
        CleanupOldPotentialPivots(currentBarIndex);
        
        // Update the current ZigZag value
        UpdateCurrentValue();
        
        // Mark as ready after we have enough data
        if (!isReady && currentBarIndex >= Depth)
        {
            isReady = true;
        }
    }

    private void UpdateTrendTracking(TOutput high, TOutput low, int currentBarIndex)
    {
        // Track the highest high and lowest low
        if (high > trendHigh || trendHigh == TOutput.CreateChecked(decimal.MinValue))
        {
            trendHigh = high;
            trendHighBarIndex = currentBarIndex;
        }
        
        if (low < trendLow || trendLow == TOutput.CreateChecked(decimal.MaxValue))
        {
            trendLow = low;
            trendLowBarIndex = currentBarIndex;
        }
    }

    private void CheckForPivots(TOutput high, TOutput low, int currentBarIndex)
    {
        // Look for potential pivot highs
        if (currentTrend != TrendState.Down && ShouldCheckForPivotHigh())
        {
            var recentHigh = FindRecentHigh(currentBarIndex);
            if (recentHigh.HasValue && 
                MeetsDeviationThreshold(recentHigh.Value.price, low) &&
                currentBarIndex - recentHigh.Value.barIndex >= Depth)
            {
                // Confirm the pivot high and look for reversal
                ConfirmPivotHigh(recentHigh.Value.price, recentHigh.Value.barIndex);
                currentTrend = TrendState.Down;
                
                // Reset trend tracking for the downtrend
                trendLow = low;
                trendLowBarIndex = currentBarIndex;
            }
        }
        
        // Look for potential pivot lows
        if (currentTrend != TrendState.Up && ShouldCheckForPivotLow())
        {
            var recentLow = FindRecentLow(currentBarIndex);
            if (recentLow.HasValue && 
                MeetsDeviationThreshold(recentLow.Value.price, high) &&
                currentBarIndex - recentLow.Value.barIndex >= Depth)
            {
                // Confirm the pivot low and look for reversal
                ConfirmPivotLow(recentLow.Value.price, recentLow.Value.barIndex);
                currentTrend = TrendState.Up;
                
                // Reset trend tracking for the uptrend
                trendHigh = high;
                trendHighBarIndex = currentBarIndex;
            }
        }
        
        // Add current bar as a potential pivot
        AddPotentialPivot(high, low, currentBarIndex);
    }

    private bool ShouldCheckForPivotHigh()
    {
        return pivotHistory.Count == 0 || 
               pivotHistory.LastOrDefault().IsHigh == false;
    }

    private bool ShouldCheckForPivotLow()
    {
        return pivotHistory.Count == 0 || 
               pivotHistory.LastOrDefault().IsHigh == true;
    }

    private (TOutput price, int barIndex)? FindRecentHigh(int currentBarIndex)
    {
        TOutput maxHigh = TOutput.CreateChecked(decimal.MinValue);
        int maxBarIndex = -1;
        
        for (int i = Math.Max(0, currentBarIndex - Depth - 10); i < currentBarIndex; i++)
        {
            var potential = potentialPivots.FirstOrDefault(p => p.BarIndex == i);
            if (potential != null && potential.High > maxHigh)
            {
                maxHigh = potential.High;
                maxBarIndex = i;
            }
        }
        
        if (maxBarIndex >= 0)
        {
            return (maxHigh, maxBarIndex);
        }
        
        return null;
    }

    private (TOutput price, int barIndex)? FindRecentLow(int currentBarIndex)
    {
        TOutput minLow = TOutput.CreateChecked(decimal.MaxValue);
        int minBarIndex = -1;
        
        for (int i = Math.Max(0, currentBarIndex - Depth - 10); i < currentBarIndex; i++)
        {
            var potential = potentialPivots.FirstOrDefault(p => p.BarIndex == i);
            if (potential != null && potential.Low < minLow)
            {
                minLow = potential.Low;
                minBarIndex = i;
            }
        }
        
        if (minBarIndex >= 0)
        {
            return (minLow, minBarIndex);
        }
        
        return null;
    }

    private void ConfirmPivotHigh(TOutput price, int barIndex)
    {
        var pivot = new ZigZagPivot<TOutput>
        {
            Price = price,
            BarIndex = barIndex,
            IsHigh = true,
            IsConfirmed = true
        };
        
        AddPivotToHistory(pivot);
        lastPivotHigh = price;
        currentDirection = -1; // Now going down
        currentZigZagValue = price;
    }

    private void ConfirmPivotLow(TOutput price, int barIndex)
    {
        var pivot = new ZigZagPivot<TOutput>
        {
            Price = price,
            BarIndex = barIndex,
            IsHigh = false,
            IsConfirmed = true
        };
        
        AddPivotToHistory(pivot);
        lastPivotLow = price;
        currentDirection = 1; // Now going up
        currentZigZagValue = price;
    }

    private void AddPotentialPivot(TOutput high, TOutput low, int barIndex)
    {
        var potential = new PotentialPivot
        {
            High = high,
            Low = low,
            BarIndex = barIndex
        };
        
        potentialPivots.Add(potential);
    }

    private void CleanupOldPotentialPivots(int currentBarIndex)
    {
        // Remove potential pivots that are too old to be relevant
        var cutoffIndex = currentBarIndex - (Depth * 3);
        potentialPivots.RemoveAll(p => p.BarIndex < cutoffIndex);
    }

    private void AddPivotToHistory(ZigZagPivot<TOutput> pivot)
    {
        pivotHistory.Add(pivot);
        
        // Maintain maximum history size
        while (pivotHistory.Count > Parameters.MaxPivotHistory)
        {
            pivotHistory.RemoveAt(0);
        }
    }

    private void UpdateCurrentValue()
    {
        // If we have confirmed pivots, use the most recent one
        if (pivotHistory.Count > 0)
        {
            var lastPivot = pivotHistory[pivotHistory.Count - 1];
            currentZigZagValue = lastPivot.Price;
        }
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        pivotHistory.Clear();
        potentialPivots.Clear();
        
        currentZigZagValue = MissingOutputValue;
        lastPivotHigh = MissingOutputValue;
        lastPivotLow = MissingOutputValue;
        currentDirection = 0;
        
        barIndex = 0;
        isReady = false;
        currentTrend = TrendState.Unknown;
        
        trendHigh = TOutput.CreateChecked(decimal.MinValue);
        trendLow = TOutput.CreateChecked(decimal.MaxValue);
        trendHighBarIndex = 0;
        trendLowBarIndex = 0;
    }

    #endregion

    #region Helper Classes

    private class PotentialPivot
    {
        public TOutput High { get; set; }
        public TOutput Low { get; set; }
        public int BarIndex { get; set; }
    }

    private enum TrendState
    {
        Unknown,
        Up,
        Down
    }

    #endregion
}
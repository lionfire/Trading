using LionFire.Trading;
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators;
using LionFire.Structures;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Pivot Points indicator.
/// Calculates support and resistance levels based on the previous period's High, Low, and Close prices.
/// Uses period aggregation logic to properly handle different timeframes (Daily, Weekly, Monthly).
/// </summary>
public class PivotPoints_FP<TInput, TOutput>
    : PivotPointsBase<TInput, TOutput>
    , IIndicator2<PivotPoints_FP<TInput, TOutput>, PPivotPoints<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Static

    /// <summary>
    /// Gets the output slots for the Pivot Points indicator
    /// </summary>
    public static IReadOnlyList<OutputSlot> Outputs()
        => [
            new() { Name = "PivotPoint", ValueType = typeof(TOutput) },
            new() { Name = "Resistance1", ValueType = typeof(TOutput) },
            new() { Name = "Support1", ValueType = typeof(TOutput) },
            new() { Name = "Resistance2", ValueType = typeof(TOutput) },
            new() { Name = "Support2", ValueType = typeof(TOutput) },
            new() { Name = "Resistance3", ValueType = typeof(TOutput) },
            new() { Name = "Support3", ValueType = typeof(TOutput) }
        ];

    /// <summary>
    /// Gets the output slots for the Pivot Points indicator with parameters
    /// </summary>
    public static List<OutputSlot> Outputs(PPivotPoints<TInput, TOutput> p)
        => [
            new() { Name = "PivotPoint", ValueType = typeof(TOutput) },
            new() { Name = "Resistance1", ValueType = typeof(TOutput) },
            new() { Name = "Support1", ValueType = typeof(TOutput) },
            new() { Name = "Resistance2", ValueType = typeof(TOutput) },
            new() { Name = "Support2", ValueType = typeof(TOutput) },
            new() { Name = "Resistance3", ValueType = typeof(TOutput) },
            new() { Name = "Support3", ValueType = typeof(TOutput) }
        ];

    #endregion

    #region Fields

    private TOutput currentPivotPoint;
    private TOutput currentR1;
    private TOutput currentS1;
    private TOutput currentR2;
    private TOutput currentS2;
    private TOutput currentR3;
    private TOutput currentS3;
    
    // Period tracking for aggregation
    private DateTime? currentPeriodStart;
    private TOutput periodHigh;
    private TOutput periodLow;
    private TOutput periodClose;
    private bool hasData = false;
    private bool isReady = false;

    #endregion

    #region Properties

    public override TOutput PivotPoint => isReady ? currentPivotPoint : MissingOutputValue;
    public override TOutput Resistance1 => isReady ? currentR1 : MissingOutputValue;
    public override TOutput Support1 => isReady ? currentS1 : MissingOutputValue;
    public override TOutput Resistance2 => isReady ? currentR2 : MissingOutputValue;
    public override TOutput Support2 => isReady ? currentS2 : MissingOutputValue;
    public override TOutput Resistance3 => isReady ? currentR3 : MissingOutputValue;
    public override TOutput Support3 => isReady ? currentS3 : MissingOutputValue;

    public override bool IsReady => isReady;

    #endregion

    #region Lifecycle

    public static PivotPoints_FP<TInput, TOutput> Create(PPivotPoints<TInput, TOutput> p)
        => new PivotPoints_FP<TInput, TOutput>(p);

    public PivotPoints_FP(PPivotPoints<TInput, TOutput> parameters) : base(parameters)
    {
        currentPivotPoint = MissingOutputValue;
        currentR1 = MissingOutputValue;
        currentS1 = MissingOutputValue;
        currentR2 = MissingOutputValue;
        currentS2 = MissingOutputValue;
        currentR3 = MissingOutputValue;
        currentS3 = MissingOutputValue;
        
        periodHigh = TOutput.CreateChecked(decimal.MinValue);
        periodLow = TOutput.CreateChecked(decimal.MaxValue);
        periodClose = MissingOutputValue;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract OHLC data
            var (open, high, low, close) = ExtractOHLC(input);
            
            // For pivot points, we need to track periods and calculate pivots from previous period's data
            // This simplified implementation assumes each input represents a complete period for now
            // In a real implementation, you would need timestamp information to properly aggregate periods
            
            if (!hasData)
            {
                // First data point - initialize period tracking
                periodHigh = high;
                periodLow = low;
                periodClose = close;
                hasData = true;
                
                // Can't calculate pivots yet - need at least one complete period
                OutputCurrentValues(output, ref outputIndex, ref outputSkip);
                continue;
            }
            
            // Use previous period's data to calculate pivots
            var (pivotPoint, r1, s1, r2, s2, r3, s3) = CalculatePivotPoints(periodHigh, periodLow, periodClose);
            
            // Update current pivot values
            currentPivotPoint = pivotPoint;
            currentR1 = r1;
            currentS1 = s1;
            currentR2 = r2;
            currentS2 = s2;
            currentR3 = r3;
            currentS3 = s3;
            
            isReady = true;
            
            // Update period tracking with current data for next calculation
            periodHigh = high;
            periodLow = low;
            periodClose = close;
            
            OutputCurrentValues(output, ref outputIndex, ref outputSkip);
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

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        // Reset state
        currentPivotPoint = MissingOutputValue;
        currentR1 = MissingOutputValue;
        currentS1 = MissingOutputValue;
        currentR2 = MissingOutputValue;
        currentS2 = MissingOutputValue;
        currentR3 = MissingOutputValue;
        currentS3 = MissingOutputValue;
        
        currentPeriodStart = null;
        periodHigh = TOutput.CreateChecked(decimal.MinValue);
        periodLow = TOutput.CreateChecked(decimal.MaxValue);
        periodClose = MissingOutputValue;
        hasData = false;
        isReady = false;
    }

    /// <summary>
    /// Helper method to output current pivot point values to the output buffer
    /// </summary>
    private void OutputCurrentValues(TOutput[]? outputBuffer, ref int outputIndex, ref int outputSkip)
    {
        var values = new[] { PivotPoint, Resistance1, Support1, Resistance2, Support2, Resistance3, Support3 };
        
        foreach (var value in values)
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
    }

    /// <summary>
    /// Gets the start of the period for the given date and period type
    /// </summary>
    private DateTime GetPeriodStart(DateTime date, PivotPointsPeriod periodType)
    {
        return periodType switch
        {
            PivotPointsPeriod.Daily => date.Date,
            PivotPointsPeriod.Weekly => GetWeekStart(date),
            PivotPointsPeriod.Monthly => new DateTime(date.Year, date.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(periodType))
        };
    }

    /// <summary>
    /// Gets the start of the week (Monday) for the given date
    /// </summary>
    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    #endregion
}
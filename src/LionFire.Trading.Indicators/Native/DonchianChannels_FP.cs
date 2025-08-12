using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Donchian Channels using circular buffers for efficiency
/// Optimized for streaming updates
/// </summary>
public class DonchianChannels_FP<TPrice, TOutput>
    : DonchianChannelsBase<DonchianChannels_FP<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<DonchianChannels_FP<TPrice, TOutput>, PDonchianChannels<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly Queue<TOutput> highWindow;
    private readonly Queue<TOutput> lowWindow;
    private TOutput currentUpperChannel;
    private TOutput currentLowerChannel;
    private TOutput currentPrice;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput UpperChannel => currentUpperChannel;
    
    public override TOutput LowerChannel => currentLowerChannel;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    public override TOutput PercentPosition 
    {
        get
        {
            if (!IsReady) return default(TOutput)!;
            
            var width = ChannelWidth;
            if (width == TOutput.Zero) return TOutput.CreateChecked(0.5);
            
            return (currentPrice - LowerChannel) / width;
        }
    }

    #endregion

    #region Lifecycle

    public static DonchianChannels_FP<TPrice, TOutput> Create(PDonchianChannels<TPrice, TOutput> p)
        => new DonchianChannels_FP<TPrice, TOutput>(p);

    public DonchianChannels_FP(PDonchianChannels<TPrice, TOutput> parameters) : base(parameters)
    {
        highWindow = new Queue<TOutput>(Parameters.Period);
        lowWindow = new Queue<TOutput>(Parameters.Period);
        currentUpperChannel = default(TOutput)!;
        currentLowerChannel = default(TOutput)!;
        currentPrice = default(TOutput)!;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type through decimal as intermediate
            var high = TOutput.CreateChecked(Convert.ToDecimal(input.High));
            var low = TOutput.CreateChecked(Convert.ToDecimal(input.Low));
            var close = TOutput.CreateChecked(Convert.ToDecimal(input.Close));
            
            currentPrice = close;
            
            // Update the sliding windows
            if (highWindow.Count >= Parameters.Period)
            {
                highWindow.Dequeue();
                lowWindow.Dequeue();
            }
            
            // Add new values
            highWindow.Enqueue(high);
            lowWindow.Enqueue(low);
            dataPointsReceived++;
            
            // Calculate channels if we have enough data
            if (IsReady)
            {
                CalculateChannels();
            }
            
            // Output the channel values
            var upper = IsReady ? currentUpperChannel : default(TOutput)!;
            var lower = IsReady ? currentLowerChannel : default(TOutput)!;
            var middle = IsReady ? MiddleChannel : default(TOutput)!;
            
            // Output in the order: Upper, Lower, Middle
            if (outputSkip > 0)
            {
                outputSkip--;
                if (outputSkip > 0) outputSkip--;
                if (outputSkip > 0) outputSkip--;
            }
            else if (output != null)
            {
                if (outputIndex < output.Length) output[outputIndex++] = upper;
                if (outputIndex < output.Length) output[outputIndex++] = lower;
                if (outputIndex < output.Length) output[outputIndex++] = middle;
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

    #region Donchian Channels Calculation

    private void CalculateChannels()
    {
        // Find the highest high and lowest low over the period
        currentUpperChannel = highWindow.Max();
        currentLowerChannel = lowWindow.Min();
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        highWindow.Clear();
        lowWindow.Clear();
        currentUpperChannel = default(TOutput)!;
        currentLowerChannel = default(TOutput)!;
        currentPrice = default(TOutput)!;
        dataPointsReceived = 0;
    }

    #endregion
}
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using LionFire.Trading.Indicators.Utils;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Rate of Change (ROC) using circular buffer optimization
/// Optimized for streaming updates
/// </summary>
public class ROC_FP<TPrice, TOutput>
    : ROCBase<TPrice, TOutput>
    , IIndicator2<ROC_FP<TPrice, TOutput>, PROC<TPrice, TOutput>, TPrice, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly CircularBuffer<TOutput> priceHistory;
    private TOutput currentROC;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentROC;
    
    public override bool IsReady => dataPointsReceived > Parameters.Period;

    #endregion

    #region Lifecycle

    public static ROC_FP<TPrice, TOutput> Create(PROC<TPrice, TOutput> p)
        => new ROC_FP<TPrice, TOutput>(p);

    public ROC_FP(PROC<TPrice, TOutput> parameters) : base(parameters)
    {
        priceHistory = new CircularBuffer<TOutput>(Parameters.Period + 1);
        currentROC = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Convert input to TOutput type through decimal as intermediate
            var price = TOutput.CreateChecked(Convert.ToDecimal(input));
            
            // Add current price to circular buffer
            priceHistory.Add(price);
            dataPointsReceived++;
            
            // Calculate ROC if we have enough data
            if (IsReady)
            {
                var currentPrice = priceHistory.Back;
                var pastPrice = priceHistory.Front;
                
                // ROC = ((Current Price - Price N periods ago) / Price N periods ago) * 100
                if (pastPrice != TOutput.Zero)
                {
                    var change = currentPrice - pastPrice;
                    var hundred = TOutput.CreateChecked(100);
                    currentROC = (change / pastPrice) * hundred;
                }
                else
                {
                    // If past price is zero, ROC is undefined, set to zero
                    currentROC = TOutput.Zero;
                }
            }
            
            var outputValue = IsReady ? currentROC : MissingOutputValue;
            
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

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        priceHistory.Clear();
        currentROC = TOutput.Zero;
        dataPointsReceived = 0;
    }

    #endregion
}
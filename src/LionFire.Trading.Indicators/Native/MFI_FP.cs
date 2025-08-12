using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Money Flow Index (MFI) using circular buffer
/// for optimal memory usage and streaming updates.
/// MFI = 100 - (100 / (1 + Money Flow Ratio))
/// where Money Flow Ratio = Positive Money Flow / Negative Money Flow
/// </summary>
public class MFI_FP<TInput, TOutput>
    : MFIBase<TInput, TOutput>
    , IIndicator2<MFI_FP<TInput, TOutput>, PMFI<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly TOutput[] positiveMoneyFlows;
    private readonly TOutput[] negativeMoneyFlows;
    private readonly TOutput[] typicalPrices;
    private int bufferIndex = 0;
    private int dataPointsReceived = 0;
    private TOutput currentMFI;
    private TOutput previousTypicalPrice;
    private bool hasPreviousTypicalPrice = false;
    
    // Running sums for efficiency
    private TOutput sumPositiveMoneyFlow;
    private TOutput sumNegativeMoneyFlow;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentMFI;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    public override TOutput PositiveMoneyFlow => sumPositiveMoneyFlow;

    public override TOutput NegativeMoneyFlow => sumNegativeMoneyFlow;

    #endregion

    #region Lifecycle

    public static MFI_FP<TInput, TOutput> Create(PMFI<TInput, TOutput> p)
        => new MFI_FP<TInput, TOutput>(p);

    public MFI_FP(PMFI<TInput, TOutput> parameters) : base(parameters)
    {
        positiveMoneyFlows = new TOutput[Parameters.Period];
        negativeMoneyFlows = new TOutput[Parameters.Period];
        typicalPrices = new TOutput[Parameters.Period];
        currentMFI = TOutput.CreateChecked(50); // Default neutral MFI
        sumPositiveMoneyFlow = TOutput.Zero;
        sumNegativeMoneyFlow = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract OHLCV data
            var (open, high, low, close, volume) = ExtractOHLCV(input);
            
            // Calculate typical price (H+L+C)/3
            var currentTypicalPrice = CalculateTypicalPrice(high, low, close);
            
            // Calculate raw money flow
            var rawMoneyFlow = CalculateRawMoneyFlow(currentTypicalPrice, volume);
            
            TOutput positiveFlow = TOutput.Zero;
            TOutput negativeFlow = TOutput.Zero;
            
            if (hasPreviousTypicalPrice)
            {
                if (currentTypicalPrice > previousTypicalPrice)
                {
                    positiveFlow = rawMoneyFlow;
                }
                else if (currentTypicalPrice < previousTypicalPrice)
                {
                    negativeFlow = rawMoneyFlow;
                }
                // If equal, both flows remain zero
                
                UpdateMFI(positiveFlow, negativeFlow);
            }
            
            // Update state
            previousTypicalPrice = currentTypicalPrice;
            hasPreviousTypicalPrice = true;
            dataPointsReceived++;
            
            var outputValue = IsReady ? currentMFI : MissingOutputValue;
            
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

    #region MFI Calculation

    private void UpdateMFI(TOutput positiveFlow, TOutput negativeFlow)
    {
        // If buffer is full, subtract the old values before adding new ones
        if (dataPointsReceived >= Parameters.Period)
        {
            var oldPositiveFlow = positiveMoneyFlows[bufferIndex];
            var oldNegativeFlow = negativeMoneyFlows[bufferIndex];
            
            sumPositiveMoneyFlow = sumPositiveMoneyFlow - oldPositiveFlow + positiveFlow;
            sumNegativeMoneyFlow = sumNegativeMoneyFlow - oldNegativeFlow + negativeFlow;
        }
        else
        {
            // Still building up the initial period
            sumPositiveMoneyFlow += positiveFlow;
            sumNegativeMoneyFlow += negativeFlow;
        }
        
        // Store new values in circular buffer
        positiveMoneyFlows[bufferIndex] = positiveFlow;
        negativeMoneyFlows[bufferIndex] = negativeFlow;
        
        // Advance buffer index (circular)
        bufferIndex = (bufferIndex + 1) % Parameters.Period;
        
        // Calculate MFI if we have enough data
        if (dataPointsReceived >= Parameters.Period)
        {
            CalculateMFI();
        }
    }
    
    private void CalculateMFI()
    {
        if (sumNegativeMoneyFlow == TOutput.Zero)
        {
            // If there are no negative money flows, MFI is 100
            currentMFI = TOutput.CreateChecked(100);
        }
        else if (sumPositiveMoneyFlow == TOutput.Zero)
        {
            // If there are no positive money flows, MFI is 0
            currentMFI = TOutput.Zero;
        }
        else
        {
            // MFI = 100 - (100 / (1 + Money Flow Ratio))
            // where Money Flow Ratio = Positive Money Flow / Negative Money Flow
            var moneyFlowRatio = sumPositiveMoneyFlow / sumNegativeMoneyFlow;
            var hundred = TOutput.CreateChecked(100);
            currentMFI = hundred - (hundred / (TOutput.One + moneyFlowRatio));
        }
        
        // Ensure MFI is within bounds [0, 100]
        var zero = TOutput.Zero;
        var hundred100 = TOutput.CreateChecked(100);
        if (currentMFI < zero) currentMFI = zero;
        if (currentMFI > hundred100) currentMFI = hundred100;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        // Clear buffers
        Array.Clear(positiveMoneyFlows, 0, positiveMoneyFlows.Length);
        Array.Clear(negativeMoneyFlows, 0, negativeMoneyFlows.Length);
        Array.Clear(typicalPrices, 0, typicalPrices.Length);
        
        // Reset state
        bufferIndex = 0;
        dataPointsReceived = 0;
        currentMFI = TOutput.CreateChecked(50);
        previousTypicalPrice = TOutput.Zero;
        hasPreviousTypicalPrice = false;
        sumPositiveMoneyFlow = TOutput.Zero;
        sumNegativeMoneyFlow = TOutput.Zero;
    }

    #endregion
}
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Accumulation/Distribution Line indicator
/// A/D Line measures cumulative flow of money in and out of a security
/// </summary>
public class AccumulationDistributionLine_FP<TInput, TOutput>
    : AccumulationDistributionLineBase<TInput, TOutput>
    , IIndicator2<AccumulationDistributionLine_FP<TInput, TOutput>, PAccumulationDistributionLine<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private TOutput accumulationDistributionLine = TOutput.Zero;
    private TOutput lastMoneyFlowVolume = TOutput.Zero;
    private TOutput lastMoneyFlowMultiplier = TOutput.Zero;
    private bool hasData = false;

    #endregion

    #region Properties

    public override TOutput CurrentValue => accumulationDistributionLine;
    
    public override TOutput LastMoneyFlowVolume => lastMoneyFlowVolume;
    
    public override TOutput LastMoneyFlowMultiplier => lastMoneyFlowMultiplier;
    
    public override bool IsReady => hasData;

    #endregion

    #region Lifecycle

    public static AccumulationDistributionLine_FP<TInput, TOutput> Create(PAccumulationDistributionLine<TInput, TOutput> p)
        => new AccumulationDistributionLine_FP<TInput, TOutput>(p);

    public AccumulationDistributionLine_FP(PAccumulationDistributionLine<TInput, TOutput> parameters) : base(parameters)
    {
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract HLCV data from input
            var (high, low, close, volume) = ExtractHLCV(input);
            
            // Calculate Money Flow Multiplier
            // MFM = ((Close - Low) - (High - Close)) / (High - Low)
            lastMoneyFlowMultiplier = CalculateMoneyFlowMultiplier(high, low, close);
            
            // Calculate Money Flow Volume
            // MFV = MFM × Volume
            lastMoneyFlowVolume = CalculateMoneyFlowVolume(lastMoneyFlowMultiplier, volume);
            
            // Update A/D Line (cumulative)
            // A/D Line = Previous A/D Line + Money Flow Volume
            accumulationDistributionLine += lastMoneyFlowVolume;
            
            hasData = true;
            
            var outputValue = IsReady ? accumulationDistributionLine : MissingOutputValue;
            
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
        accumulationDistributionLine = TOutput.Zero;
        lastMoneyFlowVolume = TOutput.Zero;
        lastMoneyFlowMultiplier = TOutput.Zero;
        hasData = false;
    }

    #endregion
}
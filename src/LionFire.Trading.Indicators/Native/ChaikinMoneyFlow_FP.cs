using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;

namespace LionFire.Trading.Indicators.Native;

/// <summary>
/// First-party implementation of Chaikin Money Flow (CMF) using circular buffer
/// for optimal memory usage and streaming updates.
/// CMF = Σ(Money Flow Volume) / Σ(Volume) over Period
/// where Money Flow Volume = Money Flow Multiplier × Volume
/// and Money Flow Multiplier = ((Close - Low) - (High - Close)) / (High - Low)
/// </summary>
public class ChaikinMoneyFlow_FP<TInput, TOutput>
    : ChaikinMoneyFlowBase<TInput, TOutput>
    , IIndicator2<ChaikinMoneyFlow_FP<TInput, TOutput>, PChaikinMoneyFlow<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly TOutput[] moneyFlowVolumes;
    private readonly TOutput[] volumes;
    private int bufferIndex = 0;
    private int dataPointsReceived = 0;
    private TOutput currentCMF;
    
    // Running sums for efficiency
    private TOutput sumMoneyFlowVolume;
    private TOutput sumVolume;

    #endregion

    #region Properties

    public override TOutput CurrentValue => currentCMF;
    
    public override bool IsReady => dataPointsReceived >= Parameters.Period;

    public override TOutput MoneyFlowVolumeSum => sumMoneyFlowVolume;

    public override TOutput VolumeSum => sumVolume;

    #endregion

    #region Lifecycle

    public static ChaikinMoneyFlow_FP<TInput, TOutput> Create(PChaikinMoneyFlow<TInput, TOutput> p)
        => new ChaikinMoneyFlow_FP<TInput, TOutput>(p);

    public ChaikinMoneyFlow_FP(PChaikinMoneyFlow<TInput, TOutput> parameters) : base(parameters)
    {
        moneyFlowVolumes = new TOutput[Parameters.Period];
        volumes = new TOutput[Parameters.Period];
        currentCMF = TOutput.Zero; // Default neutral CMF
        sumMoneyFlowVolume = TOutput.Zero;
        sumVolume = TOutput.Zero;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            // Extract HLCV data
            var (high, low, close, volume) = ExtractHLCV(input);
            
            // Calculate Money Flow Multiplier
            var moneyFlowMultiplier = CalculateMoneyFlowMultiplier(high, low, close);
            
            // Calculate Money Flow Volume
            var moneyFlowVolume = CalculateMoneyFlowVolume(moneyFlowMultiplier, volume);
            
            UpdateCMF(moneyFlowVolume, volume);
            
            dataPointsReceived++;
            
            var outputValue = IsReady ? currentCMF : MissingOutputValue;
            
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

    #region CMF Calculation

    private void UpdateCMF(TOutput moneyFlowVolume, TOutput volume)
    {
        // If buffer is full, subtract the old values before adding new ones
        if (dataPointsReceived >= Parameters.Period)
        {
            var oldMoneyFlowVolume = moneyFlowVolumes[bufferIndex];
            var oldVolume = volumes[bufferIndex];
            
            sumMoneyFlowVolume = sumMoneyFlowVolume - oldMoneyFlowVolume + moneyFlowVolume;
            sumVolume = sumVolume - oldVolume + volume;
        }
        else
        {
            // Still building up the initial period
            sumMoneyFlowVolume += moneyFlowVolume;
            sumVolume += volume;
        }
        
        // Store new values in circular buffer
        moneyFlowVolumes[bufferIndex] = moneyFlowVolume;
        volumes[bufferIndex] = volume;
        
        // Advance buffer index (circular)
        bufferIndex = (bufferIndex + 1) % Parameters.Period;
        
        // Calculate CMF if we have enough data
        if (dataPointsReceived >= Parameters.Period)
        {
            CalculateCMF();
        }
    }
    
    private void CalculateCMF()
    {
        if (sumVolume == TOutput.Zero)
        {
            // If there is no volume, CMF is 0
            currentCMF = TOutput.Zero;
        }
        else
        {
            // CMF = Σ(Money Flow Volume) / Σ(Volume) over Period
            currentCMF = sumMoneyFlowVolume / sumVolume;
        }
        
        // Ensure CMF is within theoretical bounds [-1, +1]
        var one = TOutput.One;
        var minusOne = TOutput.Zero - TOutput.One;
        
        if (currentCMF > one) 
            currentCMF = one;
        else if (currentCMF < minusOne) 
            currentCMF = minusOne;
    }

    #endregion

    #region Methods

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        
        // Clear buffers
        Array.Clear(moneyFlowVolumes, 0, moneyFlowVolumes.Length);
        Array.Clear(volumes, 0, volumes.Length);
        
        // Reset state
        bufferIndex = 0;
        dataPointsReceived = 0;
        currentCMF = TOutput.Zero;
        sumMoneyFlowVolume = TOutput.Zero;
        sumVolume = TOutput.Zero;
    }

    #endregion
}
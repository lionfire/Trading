using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Fisher Transform
/// Falls back to FP implementation since QuantConnect doesn't have built-in Fisher Transform
/// </summary>
public class FisherTransform_QC<TPrice, TOutput> 
    : FisherTransformBase<TPrice, TOutput>
    , IIndicator2<FisherTransform_QC<TPrice, TOutput>, PFisherTransform<TPrice, TOutput>, HL<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly FisherTransform_FP<TPrice, TOutput> fallbackImplementation;
    private readonly bool isQuantConnectAvailable = false; // Fisher Transform not in standard QC indicators

    #endregion

    #region Properties

    public override TOutput Fisher => fallbackImplementation.Fisher;
    
    public override TOutput Trigger => fallbackImplementation.Trigger;
    
    public override bool IsReady => fallbackImplementation.IsReady;

    #endregion

    #region Lifecycle

    public static FisherTransform_QC<TPrice, TOutput> Create(PFisherTransform<TPrice, TOutput> p)
        => new FisherTransform_QC<TPrice, TOutput>(p);

    public FisherTransform_QC(PFisherTransform<TPrice, TOutput> parameters) : base(parameters)
    {
        // Try to find QuantConnect Fisher Transform indicator
        try
        {
            // Currently QuantConnect does not have a built-in Fisher Transform indicator
            // So we fall back to our FP implementation
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var fisherType = qcAssembly.GetType("QuantConnect.Indicators.FisherTransform");
            
            if (fisherType != null)
            {
                // If QuantConnect adds Fisher Transform in the future, implement it here
                isQuantConnectAvailable = false; // Set to true when available
            }
        }
        catch
        {
            // QuantConnect assembly not available or Fisher Transform not found
        }
        
        // Always use our FP implementation for now
        fallbackImplementation = new FisherTransform_FP<TPrice, TOutput>(parameters);
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HL<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        // For now, always delegate to our FP implementation
        // In the future, if QuantConnect adds Fisher Transform, we can add conditional logic here
        fallbackImplementation.OnBarBatch(inputs, output, outputIndex, outputSkip);
        
        // Forward observer notifications
        if (subject != null && output != null && outputIndex > 0)
        {
            var results = new TOutput[outputIndex];
            Array.Copy(output, results, outputIndex);
            subject.OnNext(results);
        }
    }

    #endregion

    #region Cleanup

    public override void Clear()
    {
        subject?.OnCompleted();
        subject = null;
        fallbackImplementation.Clear();
    }

    #endregion
}
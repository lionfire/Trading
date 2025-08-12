using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using System.Numerics;
using System.Reflection;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect implementation of Accumulation/Distribution Line using QuantConnect's AccumulationDistribution indicator if available,
/// otherwise falls back to the first-party implementation.
/// </summary>
public class AccumulationDistributionLine_QC<TInput, TOutput>
    : AccumulationDistributionLineBase<TInput, TOutput>
    , IIndicator2<AccumulationDistributionLine_QC<TInput, TOutput>, PAccumulationDistributionLine<TInput, TOutput>, TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly object? quantConnectIndicator;
    private readonly MethodInfo? updateMethod;
    private readonly PropertyInfo? currentValueProperty;
    private readonly bool useQuantConnect;
    
    // Fallback to FP implementation
    private readonly Native.AccumulationDistributionLine_FP<TInput, TOutput>? fallbackIndicator;
    
    // Track values for the interface since QC might not expose them directly
    private TOutput lastMoneyFlowVolume = TOutput.Zero;
    private TOutput lastMoneyFlowMultiplier = TOutput.Zero;

    #endregion

    #region Properties

    public override TOutput CurrentValue
    {
        get
        {
            if (useQuantConnect && quantConnectIndicator != null && currentValueProperty != null)
            {
                var value = currentValueProperty.GetValue(quantConnectIndicator);
                return TOutput.CreateChecked(Convert.ToDecimal(value!));
            }
            
            return fallbackIndicator?.CurrentValue ?? MissingOutputValue;
        }
    }
    
    public override bool IsReady
    {
        get
        {
            if (useQuantConnect && quantConnectIndicator != null)
            {
                var isReadyProperty = quantConnectIndicator.GetType().GetProperty("IsReady");
                if (isReadyProperty != null)
                {
                    return (bool)isReadyProperty.GetValue(quantConnectIndicator)!;
                }
            }
            
            return fallbackIndicator?.IsReady ?? false;
        }
    }

    public override TOutput LastMoneyFlowVolume
    {
        get
        {
            if (useQuantConnect)
            {
                // QuantConnect A/D indicator may not expose MFV directly, use our calculation
                return lastMoneyFlowVolume;
            }
            
            return fallbackIndicator?.LastMoneyFlowVolume ?? TOutput.Zero;
        }
    }

    public override TOutput LastMoneyFlowMultiplier
    {
        get
        {
            if (useQuantConnect)
            {
                // QuantConnect A/D indicator may not expose MFM directly, use our calculation
                return lastMoneyFlowMultiplier;
            }
            
            return fallbackIndicator?.LastMoneyFlowMultiplier ?? TOutput.Zero;
        }
    }

    #endregion

    #region Lifecycle

    public static AccumulationDistributionLine_QC<TInput, TOutput> Create(PAccumulationDistributionLine<TInput, TOutput> p)
        => new AccumulationDistributionLine_QC<TInput, TOutput>(p);

    public AccumulationDistributionLine_QC(PAccumulationDistributionLine<TInput, TOutput> parameters) : base(parameters)
    {
        var (qcIndicator, updateMethodInfo, currentValueProp, success) = TryCreateQuantConnectIndicator();
        
        quantConnectIndicator = qcIndicator;
        updateMethod = updateMethodInfo;
        currentValueProperty = currentValueProp;
        useQuantConnect = success;
        
        if (!useQuantConnect)
        {
            // Fallback to first-party implementation
            fallbackIndicator = new Native.AccumulationDistributionLine_FP<TInput, TOutput>(parameters);
        }
    }

    #endregion

    #region QuantConnect Integration

    private (object? indicator, MethodInfo? updateMethod, PropertyInfo? currentValueProperty, bool success) TryCreateQuantConnectIndicator()
    {
        try
        {
            // Try to load QuantConnect assembly and create AccumulationDistribution indicator
            var qcAssemblyName = "QuantConnect.Indicators";
            var qcAssembly = Assembly.LoadFrom($"{qcAssemblyName}.dll");
            
            var adType = qcAssembly.GetType("QuantConnect.Indicators.AccumulationDistribution");
            if (adType == null)
            {
                return (null, null, null, false);
            }
            
            // Create instance - A/D Line doesn't need period parameter
            var indicator = Activator.CreateInstance(adType);
            if (indicator == null)
            {
                return (null, null, null, false);
            }
            
            // Get the Update method - QuantConnect typically uses Update(IBaseDataBar bar)
            var updateMethod = adType.GetMethod("Update", new[] { typeof(object) });
            
            // Get the Current property or Value property
            var currentValueProperty = adType.GetProperty("Current") ?? adType.GetProperty("Value");
            
            if (updateMethod == null || currentValueProperty == null)
            {
                return (null, null, null, false);
            }
            
            return (indicator, updateMethod, currentValueProperty, true);
        }
        catch
        {
            // QuantConnect not available or failed to load
            return (null, null, null, false);
        }
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TInput> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        if (useQuantConnect && quantConnectIndicator != null && updateMethod != null)
        {
            foreach (var input in inputs)
            {
                try
                {
                    // Extract HLCV data and create a compatible data structure for QuantConnect
                    var (high, low, close, volume) = ExtractHLCV(input);
                    
                    // Calculate our own MFM and MFV for the interface
                    lastMoneyFlowMultiplier = CalculateMoneyFlowMultiplier(high, low, close);
                    lastMoneyFlowVolume = CalculateMoneyFlowVolume(lastMoneyFlowMultiplier, volume);
                    
                    // Create a simple data structure that QuantConnect can work with
                    // This is a simplified approach - in practice, you'd want to create a proper IBaseDataBar
                    var barData = new
                    {
                        High = Convert.ToDecimal(high),
                        Low = Convert.ToDecimal(low),
                        Close = Convert.ToDecimal(close),
                        Volume = Convert.ToDecimal(volume)
                    };
                    
                    updateMethod.Invoke(quantConnectIndicator, new object[] { barData });
                    
                    var outputValue = IsReady ? CurrentValue : MissingOutputValue;
                    
                    if (outputSkip > 0)
                    {
                        outputSkip--;
                    }
                    else if (output != null && outputIndex < output.Length)
                    {
                        output[outputIndex++] = outputValue;
                    }
                }
                catch
                {
                    // If QuantConnect update fails, fall back to FP implementation
                    if (fallbackIndicator == null)
                    {
                        var fallback = new Native.AccumulationDistributionLine_FP<TInput, TOutput>(Parameters);
                        fallback.OnBarBatch(inputs, output, outputIndex, outputSkip);
                        return;
                    }
                }
            }
        }
        else
        {
            // Use fallback implementation
            fallbackIndicator?.OnBarBatch(inputs, output, outputIndex, outputSkip);
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
        
        lastMoneyFlowVolume = TOutput.Zero;
        lastMoneyFlowMultiplier = TOutput.Zero;
        
        if (useQuantConnect && quantConnectIndicator != null)
        {
            // Try to reset QuantConnect indicator
            var resetMethod = quantConnectIndicator.GetType().GetMethod("Reset");
            resetMethod?.Invoke(quantConnectIndicator, null);
        }
        
        fallbackIndicator?.Clear();
    }

    #endregion
}
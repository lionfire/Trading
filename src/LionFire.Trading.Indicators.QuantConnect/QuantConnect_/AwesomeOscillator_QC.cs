using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Awesome Oscillator
/// Wraps the QuantConnect AwesomeOscillator indicator with LionFire interfaces
/// </summary>
public class AwesomeOscillator_QC<TPrice, TOutput> 
    : AwesomeOscillatorBase<AwesomeOscillator_QC<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<AwesomeOscillator_QC<TPrice, TOutput>, PAwesomeOscillator<TPrice, TOutput>, HLC<TPrice>, TOutput>
    where TPrice : struct
    where TOutput : struct, INumber<TOutput>
{
    #region Fields

    private readonly dynamic qcIndicator;
    private TOutput currentValue;
    private int dataPointsReceived = 0;

    #endregion

    #region Properties

    public override TOutput Value => IsReady ? currentValue : default(TOutput)!;
    
    public override bool IsReady => dataPointsReceived >= MaxLookback;

    #endregion

    #region Lifecycle

    public static AwesomeOscillator_QC<TPrice, TOutput> Create(PAwesomeOscillator<TPrice, TOutput> p)
        => new AwesomeOscillator_QC<TPrice, TOutput>(p);

    public AwesomeOscillator_QC(PAwesomeOscillator<TPrice, TOutput> parameters) : base(parameters)
    {
        try
        {
            // Try to create QuantConnect AwesomeOscillator indicator
            // AO constructor typically takes (string name, int fastPeriod, int slowPeriod)
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var aoType = qcAssembly.GetType("QuantConnect.Indicators.AwesomeOscillator");
            
            if (aoType != null)
            {
                qcIndicator = Activator.CreateInstance(aoType, 
                    $"AO({parameters.FastPeriod},{parameters.SlowPeriod})", 
                    parameters.FastPeriod, 
                    parameters.SlowPeriod);
            }
            else
            {
                throw new InvalidOperationException("QuantConnect AwesomeOscillator indicator not found");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create QuantConnect AwesomeOscillator: {ex.Message}", ex);
        }
        
        currentValue = default(TOutput)!;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<HLC<TPrice>> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            try
            {
                // Calculate median price: (High + Low) / 2
                var high = ConvertToDecimal(input.High);
                var low = ConvertToDecimal(input.Low);
                var medianPrice = (high + low) / 2m;
                
                // Create IBaseData-like object for QuantConnect indicator
                var qcBar = CreateQuantConnectBar(medianPrice);
                
                // Update the QuantConnect indicator
                qcIndicator.Update(qcBar);
                
                dataPointsReceived++;
                
                // Get the result and convert to TOutput
                if (qcIndicator.IsReady)
                {
                    var qcValue = (decimal)qcIndicator.Current.Value;
                    currentValue = TOutput.CreateChecked(qcValue);
                }
                
                // Output values
                var outputValue = IsReady ? currentValue : MissingOutputValue;
                
                if (outputSkip > 0)
                {
                    outputSkip--;
                }
                else if (output != null && outputIndex < output.Length)
                {
                    output[outputIndex++] = outputValue;
                }
                
                // Notify observers
                if (subject != null && IsReady)
                {
                    subject.OnNext(new List<TOutput> { currentValue });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing bar in AwesomeOscillator_QC: {ex.Message}", ex);
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Convert input type to decimal for QuantConnect processing
    /// </summary>
    private static decimal ConvertToDecimal(TPrice input)
    {
        return input switch
        {
            decimal d => d,
            double db => (decimal)db,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => Convert.ToDecimal(input)
        };
    }

    /// <summary>
    /// Create a QuantConnect-compatible bar object
    /// </summary>
    private dynamic CreateQuantConnectBar(decimal value)
    {
        try
        {
            // Create a simple data point for the QuantConnect indicator
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Common");
            var indicatorDataPointType = qcAssembly.GetType("QuantConnect.Data.Market.IndicatorDataPoint");
            
            if (indicatorDataPointType != null)
            {
                var now = DateTime.UtcNow;
                return Activator.CreateInstance(indicatorDataPointType, now, value);
            }
            
            // Fallback: create anonymous object with required properties
            return new { Time = DateTime.UtcNow, Value = value };
        }
        catch
        {
            // Final fallback: simple anonymous object
            return new { Time = DateTime.UtcNow, Value = value };
        }
    }

    #endregion

    #region Cleanup

    public override void Clear()
    {
        base.Clear();
        qcIndicator?.Reset();
        currentValue = default(TOutput)!;
        dataPointsReceived = 0;
    }

    #endregion
}
using LionFire.Trading.Indicators.Base;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Structures;
using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.QuantConnect_;

/// <summary>
/// QuantConnect-based implementation of Standard Deviation
/// Wraps the QuantConnect StandardDeviation indicator with LionFire interfaces
/// </summary>
public class StandardDeviation_QC<TPrice, TOutput> 
    : StandardDeviationBase<StandardDeviation_QC<TPrice, TOutput>, TPrice, TOutput>
    , IIndicator2<StandardDeviation_QC<TPrice, TOutput>, PStandardDeviation<TPrice, TOutput>, TPrice, TOutput>
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

    public static StandardDeviation_QC<TPrice, TOutput> Create(PStandardDeviation<TPrice, TOutput> p)
        => new StandardDeviation_QC<TPrice, TOutput>(p);

    public StandardDeviation_QC(PStandardDeviation<TPrice, TOutput> parameters) : base(parameters)
    {
        try
        {
            // Try to create QuantConnect StandardDeviation indicator
            // StandardDeviation constructor typically takes (string name, int period)
            var qcAssembly = System.Reflection.Assembly.Load("QuantConnect.Indicators");
            var stdDevType = qcAssembly.GetType("QuantConnect.Indicators.StandardDeviation");
            
            if (stdDevType != null)
            {
                qcIndicator = Activator.CreateInstance(stdDevType, 
                    $"STDDEV({parameters.Period})", 
                    parameters.Period);
            }
            else
            {
                throw new InvalidOperationException("QuantConnect StandardDeviation indicator not found");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create QuantConnect StandardDeviation: {ex.Message}", ex);
        }
        
        currentValue = default(TOutput)!;
    }

    #endregion

    #region Event Handling

    public override void OnBarBatch(IReadOnlyList<TPrice> inputs, TOutput[]? output, int outputIndex = 0, int outputSkip = 0)
    {
        foreach (var input in inputs)
        {
            try
            {
                // Convert input to decimal for QuantConnect processing
                var inputValue = ConvertToDecimal(input);
                
                // Create IBaseData-like object for QuantConnect indicator
                var qcDataPoint = CreateQuantConnectDataPoint(inputValue);
                
                // Update the QuantConnect indicator
                qcIndicator.Update(qcDataPoint);
                
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
                throw new InvalidOperationException($"Error processing input in StandardDeviation_QC: {ex.Message}", ex);
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
    /// Create a QuantConnect-compatible data point object
    /// </summary>
    private dynamic CreateQuantConnectDataPoint(decimal value)
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
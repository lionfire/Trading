using System.Linq;

namespace LionFire.Trading.DataFlow;

public interface IPHydratedInput : IPInput
{
    TimeFrame TimeFrame { get; }
}

public class PBoundInput : IPInput
{
    #region Relationships

    public IPUnboundInput PUnboundInput { get; }
    
    public IReadOnlyList<IPInput> Signals { get; }

    #endregion

    #region Identity

    public Type ValueType => PUnboundInput.ValueType;
    public string Key => Signals?.Count switch
    {
        null => PUnboundInput.Key,
        0 => PUnboundInput.Key,
        1 => Signals[0].Key + " > " + PUnboundInput.Key,
        _ => "[" + string.Join(" ", Signals.Select(s => s.Key)) + "] > " + PUnboundInput.Key,
    };

    #endregion

    #region Parameters

    /// <summary>
    /// TimeFrame of whoever is iterating bars.
    /// If the PUnboundInput.TimeFrame is different, it should be adapted to match this TimeFrame:
    /// - If PUnboundInput.TimeFrame is more granular: it should be rolled up somehow, if possible
    /// - If PUnboundInput.TimeFrame is more coarse: the same value should be repeated for all granular bars within the coarse bar
    /// </summary>
    public TimeFrame TimeFrame { get; set; }

    #endregion

    #region Lifecycle

    // REVIEW - can there be multiple upstream Inputs that are adapted to match the parent's exected Input type?
    public PBoundInput(IPUnboundInput unboundInput, IPTimeFrameMarketProcessor root)
    {
        
        PUnboundInput = unboundInput;

        Signals = ResolveSignals(unboundInput, root);
        TimeFrame = root.TimeFrame;

        #region Validation

        if (unboundInput.InputSlots.Count != Signals.Count)
        {
            throw new UnreachableCodeException($"Mismatched number of upstream inputs for {unboundInput}.  Expected {unboundInput.InputSlots.Count}, got {Signals.Count}");
        }

        #endregion
    }

    public const bool SkipNullSourceSignals = true;

    List<IPInput> ResolveSignals(IPUnboundInput pUnboundInput, IPMarketProcessor source)
    {
        List<IPInput> signals = new();

        var sourcesInfo = SourcesInfo.GetSourcesInfo(source.GetType());
        var slotsInfo = SlotsInfo.GetSlotsInfo(pUnboundInput.GetType());

        var signalInfos = InputSlotsReflection.GetSignalInfos(source.GetType());

        if(signalInfos.Count < pUnboundInput.InputSlots.Count)
        {
            throw new UnreachableCodeException($"Mismatched number of inputs for {source}.  Expected at least {pUnboundInput.InputSlots.Count}, got {signalInfos.Count}");
        }

        int sourceIndex = 0;

        foreach (var slot in pUnboundInput.InputSlots)
        {
            nextSourceSignal:
            var signalInfo = signalInfos[sourceIndex++];

            var signalValueType = IReferenceToX.GetTypeOfReferenced(signalInfo.PropertyInfo.PropertyType);

            if (signalValueType != ValueType)
            {
                throw new NotImplementedException($"Signal value type '{signalValueType}' does not match {nameof(PUnboundInput)}.{nameof(ValueType)} and no adapter has been implemented");
            }

            var signalPInput = (IPInput?) signalInfo.PropertyInfo.GetValue(source);

            if (signalPInput == null)
            {
                if (SkipNullSourceSignals)
                {
                    // REVIEW idea: allow null Signals, and fall back to the next one, allowing for sparsely defined signals in the Source.
                    goto nextSourceSignal;
                }
                else
                {
                    throw new ArgumentException($"Signal {signalInfo.PropertyInfo.Name} is null");
                }
            }
            if (signalPInput is IPUnboundInput unbound)
            {
                throw new NotImplementedException($"Unbound input {unbound} is not allowed as a signalInfo");
            }

            signals.Add(signalPInput);

        }

        return signals;
    }

    #endregion


}



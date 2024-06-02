using System.Linq;

namespace LionFire.Trading.DataFlow;

public class PBoundInput : IPInput
{
    #region Relationships

    public IPUnboundInput PUnboundInput { get; }
    public IReadOnlyList<IPInput> Signals { get; }

    #endregion

    #region Identity

    public Type ValueType => PUnboundInput.ValueType;

    #endregion
    public string Key => PUnboundInput.Key;

    #region Lifecycle

    // REVIEW - can there be multiple upstream Inputs that are adapted to match the parent's exected Input type?
    public PBoundInput(IPUnboundInput unboundInput, IPMarketProcessor root)
    {
        PUnboundInput = unboundInput;
        Signals = ResolveSignals(unboundInput, root);

        #region Validation

        if (unboundInput.InputSlots.Count != Signals.Count)
        {
            throw new UnreachableCodeException($"Mismatched number of upstream inputs for {unboundInput}");
        }

        #endregion
    }

    List<IPInput> ResolveSignals(IPUnboundInput pUnboundInput, IPMarketProcessor root)
    {
        List<IPInput> signals = new();

        var sourcesInfo = SourcesInfo.GetSourcesInfo(root.GetType());
        var slotsInfo = SlotsInfo.GetSlotsInfo(pUnboundInput.GetType());

        int i = 0;
        foreach (var slot in pUnboundInput.InputSlots)
        {
            var signal = Signals[i];

            if (signal is IPUnboundInput unbound)
            {
                throw new NotImplementedException($"Unbound input {unbound} is not allowed as a signal");
            }

            if (signal.ValueType != ValueType)
            {
                throw new NotImplementedException($"{nameof(signal)}.{nameof(signal.ValueType)} does not match {nameof(PUnboundInput)}.{nameof(ValueType)} and no adapter is available");
            }
            

            i++;
        }

        return signals;
    }

    #endregion


}



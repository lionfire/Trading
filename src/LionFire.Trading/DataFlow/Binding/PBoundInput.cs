using System.Linq;

namespace LionFire.Trading.DataFlow;

public interface IPHydratedInput : IPInput
{
    TimeFrame TimeFrame { get; }
}

// TODO REVIEW - this class can be replaced by the static method ResolveSlotValues
// REVIEW - Are indexed slots working for slot count > 1?
public class PBoundSlots
// Must not implement IPMayHaveUnboundInputSlots
{
    public IPMayHaveUnboundInputSlots Parent { get; }

    public PBoundSlots(IPMayHaveUnboundInputSlots unboundInput, IPMarketProcessor root)
    {
        Parent = unboundInput;
        Signals = ResolveSlotValues(unboundInput, root);

        #region Validation

        if (unboundInput.InputSlots.Count != Signals.Count)
        {
            throw new UnreachableCodeException($"Mismatched number of upstream inputs for {unboundInput}.  Expected {unboundInput.InputSlots.Count}, got {Signals.Count}");
        }

        #endregion
    }

    #region Relationships

    public IReadOnlyList<IPInput?> Signals { get; }

    #endregion

    public const bool SkipNullSourceSignals = true;

    /// <summary>
    /// Use the source's signals to resolve the unbound input's slots.
    /// </summary>
    /// <param name="hasUnboundSlots"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="UnreachableCodeException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static List<IPInput?> ResolveSlotValues(IPMayHaveUnboundInputSlots hasUnboundSlots, IPMarketProcessor source)
    {
        var signalInfos = SignalInfoReflection.GetSignalInfos(source.GetType());
        var slotInfos = SlotsInfo.GetSlotsInfo(hasUnboundSlots.GetType());

        #region Validation
        if (signalInfos.Count < hasUnboundSlots.InputSlots.Count)
        {
            throw new UnreachableCodeException($"Mismatched number of inputs for {source}.  Expected at least {hasUnboundSlots.InputSlots.Count}, got {signalInfos.Count}");
        }
        #endregion

        List<IPInput> valuesForSlots = new();

        int sourceIndex = 0;
        foreach (var slot in hasUnboundSlots.InputSlots)
        {
        // TODO: Only get values for Slots that currently are unset, and return null for ones that are.
        //var slotInfo = slotInfos.Slots.Where(s => s.ParameterProperty!.Name == slot.Name).Single();
        //if (slotInfo.ParameterProperty!.GetValue(hasUnboundSlots) != null) valuesForSlots.Add(null);

        nextSourceSignal:
            var signalInfo = signalInfos[sourceIndex++];

            #region Validation/Coerce: ValueType compatibility

            var signalValueType = IReferenceToX.GetTypeOfReferenced(signalInfo.PropertyInfo.PropertyType);
            if (signalValueType != slot.ValueType) // OLD: was  valueType but that looks wrong (parent)
            //if (signalValueType != IReferenceToX.GetTypeOfReferenced(slot.ValueType)) // OLD: was  valueType but that looks wrong (parent)
            {
                throw new NotImplementedException($"Signal value type '{signalValueType}' does not match {nameof(slot)}.{nameof(slot.ValueType)} and no adapter has been implemented"); // FUTURE: Adapter
            }

            #endregion

            var signalPInput = (IPInput?)signalInfo.PropertyInfo.GetValue(source);

            if (signalPInput == null)
            {
                if (SkipNullSourceSignals)
                {
                    // REVIEW idea: allow null Signals, and fall back to the next one, allowing for sparsely defined signals in the Source.
                    goto nextSourceSignal;
                }
                else
                {
                    throw new ArgumentException($"Signal {signalInfo.PropertyInfo.Name} is null and {SkipNullSourceSignals} so resolution fails.");
                }
            }

            if (signalPInput is IPMayHaveUnboundInputSlots unbound)
            {
                throw new NotImplementedException($"Potentially Unbound input '{unbound}' is not supported yet as a signalInfo. TODO: Support if slots have values.");
            }

            valuesForSlots.Add(signalPInput);
        }

        return valuesForSlots;
    }
}

public class PBoundInput : PBoundSlots, IPInput
{
    #region Relationships

    public IPInputThatSupportsUnboundInputs PUnboundInput => (IPInputThatSupportsUnboundInputs)Parent;

    #endregion

    #region Identity

    public Type ValueType => PUnboundInput.ValueType;

    // REVIEW / Document: what is going on here?  If there are 0 signals, it would seem that this PBountInput can't exist in a valid state, so maybe null and 0 should indicate this with some sort of " {UNBOUND}" marker
    public string Key => Signals?.Count switch
    {
        null => PUnboundInput.Key,
        0 => PUnboundInput.Key,
        1 => Signals[0]!.Key + " > " + PUnboundInput.Key,
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
    public TimeFrame? TimeFrame { get; set; }

    #endregion

    #region Lifecycle

    // REVIEW - can there be multiple upstream Inputs that are adapted to match the parent's expected Input type?
    public PBoundInput(IPInputThatSupportsUnboundInputs unboundInput, IPMarketProcessor root) : base(unboundInput, root)
    {
        if (root is IPTimeFrameMarketProcessor rootTimeFrame)
        {
            TimeFrame = rootTimeFrame.TimeFrame;
        }
    }

    #endregion
}



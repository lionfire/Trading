namespace LionFire.Trading.DataFlow; // TODO: Move to .DataFlow namespace

public interface IPUnboundInput : IPInput
{
    IReadOnlyList<InputSlot> InputSlots { get; }
    TimeFrame? TimeFrame { get; }
}



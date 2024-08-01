namespace LionFire.Trading.DataFlow;


public interface IPMayHaveUnboundInputSlots
{
    IReadOnlyList<InputSlot> InputSlots { get; }

    // TODO: Reconcile with:
    //IReadOnlyList<???> InputSlots => SlotsInfo.GetSlotsInfo(typeof(PBacktestAccount<T>)).Slots;
}

public interface IPInputThatSupportsUnboundInputs : IPInput, IPMayHaveUnboundInputSlots
{
    TimeFrame? TimeFrame { get; }
}



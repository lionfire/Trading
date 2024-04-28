namespace LionFire.Trading.IO;

public interface IInputComponent
{
    static abstract IReadOnlyList<InputSlot> InputSlots();
    IReadOnlyList<IInputSignal> InputSignals { get; }
}

public abstract class InputComponent 
    // : IInputComponent
{
    //public static abstract IReadOnlyList<InputSlot> InputSlots();

    public required IReadOnlyList<IInputSignal> InputSignals { get; init; }
}


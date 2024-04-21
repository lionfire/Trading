namespace LionFire.Trading.IO;

public interface IInputComponent
{
    static abstract IReadOnlyList<InputSlot> TInputs();
    IReadOnlyList<InputSignal> Inputs { get; }
}

public class InputComponent : IInputComponent
{
    public required IReadOnlyList<InputSlot> Inputs { get; init; }
}


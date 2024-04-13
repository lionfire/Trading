namespace LionFire.Trading.IO;

public interface IInputComponent
{
    IReadOnlyList<InputSlot> Inputs { get; }
}

public class InputComponent : IInputComponent
{
    public required IReadOnlyList<InputSlot> Inputs { get; init; }
}


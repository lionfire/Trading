using LionFire.Trading.IO;

namespace LionFire.Trading.IO;

public class IOComponent : InputComponent, IOutputComponent
{
    public required IReadOnlyList<OutputSlot> Outputs { get; init; }
}

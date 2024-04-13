namespace LionFire.Trading.IO;

public interface IOutputComponent
{
    IReadOnlyList<OutputSlot> Outputs { get; }
}

public class OutputComponent : IOutputComponent
{
    public required IReadOnlyList<OutputSlot> Outputs { get; init; }
}

namespace LionFire.Trading;

/// <summary>
/// An input or output slot for a component
/// </summary>
public class Slot
{
    public required string Name { get; init; }
    public required Type Type { get; init; }
    //public bool? TimeIndexed { get; init; }
    //public bool? TimeGaps { get; init; }
}

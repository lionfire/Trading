namespace LionFire.Trading;

/// <summary>
/// An input or output slot for a component
/// </summary>
public class Slot
{
    public required string Name { get; init; }
    public required Type Type { get; init; }


    // REVIEW: this may not be the place for it, but how do we determine time series with gaps vs without?
    //public bool? TimeGaps { get; init; }
}

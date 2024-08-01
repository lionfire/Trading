namespace LionFire.Trading;

// REVIEW: Make Slot this generic, with ISlot interface?
// REVIEW: Separate into different classes?

/// <summary>
/// An input or output slot for a component
/// </summary>
public class Slot
{
    public required string Name { get; init; }
    public required Type ValueType { get; init; }

    /// <summary>
    /// If Type is a Bar (IKline) this specifies which aspect(s) of the bar is needed. 
    /// </summary>
    public DataPointAspect Aspects { get; init; }

    // REVIEW: this may not be the place for it, but how do we determine time series with gaps vs without?
    //public bool? TimeGaps { get; init; }

    /// <summary>
    /// If volume is required, this provides info about exactly what kind(s) of volume are required, and if any fallbacks are ok.
    /// </summary>
    public VolumePolicy? VolumePolicy { get; init; }
}

// FUTURE
public class VolumePolicy
{
}

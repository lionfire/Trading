namespace LionFire.Trading;

public enum SlotType
{
    Unspecified = 0,

    /// <summary>
    /// Typical input for trading: each point in time (that is aligned with a TimeFrame) has a value
    /// </summary>
    TimeSeries = 1,

    /// <summary>
    /// Unknown way of getting data that is not aligned to a point in time in step with a TimeFrame, or perhaps even any time.
    /// </summary>
    Custom = 2, 
}

using Orleans;

namespace LionFire.Trading.Data;

[GenerateSerializer]
[Alias("SeriesSubscriptionOptions")]
public class SeriesSubscriptionOptions
{
    /// <summary>
    /// If notification is enabled, the number of bars to send upon subscription
    /// </summary>
    [Id(0)]
    public int CatchUp { get; set; } = 30;

    /// <summary>
    /// The number of bars to retain in memory, so that it can be queried manually without having to load from the disk or network
    /// </summary>
    [Id(1)]
    public int Memory { get; set; } = 0;

    /// <summary>
    /// If true, the entire memory will be sent to the subscriber with every notification
    /// </summary>
    [Id(2)]
    public bool SendWithMemory { get; set; } = false;

    ///// <summary>
    ///// If false, this is only a declaration of interest, and acquiring the data is intended to occur later manually at the request of the subscriber.
    ///// </summary>
    //[Id(3)]
    //public bool Notify { get; set; } = true;
}

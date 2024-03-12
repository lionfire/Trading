#nullable enable
namespace LionFire.Trading;

public interface IRangeWithTimeFrame
{
    TimeFrame TimeFrame { get; }
    DateTimeOffset Start { get; }
    DateTimeOffset EndExclusive { get; }
}


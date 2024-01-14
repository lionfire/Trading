#nullable enable
namespace LionFire.Trading;

public interface IRangeWithTimeFrame
{
    TimeFrame TimeFrame { get; }
    DateTime Start { get; }
    DateTime EndExclusive { get; }
}


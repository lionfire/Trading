using Orleans;

namespace LionFire.Trading;

/// <summary>
/// Orleans serialization surrogate for <see cref="TimeFrame"/>.
/// </summary>
[GenerateSerializer]
[Alias("timeframe")]
public struct TimeFrame_Surrogate
{
    [Id(0)]
    public string ShortName { get; set; }
}

/// <summary>
/// Converts between <see cref="TimeFrame"/> and <see cref="TimeFrame_Surrogate"/> for Orleans serialization.
/// </summary>
[RegisterConverter]
public sealed class TimeFrame_SurrogateConverter : IConverter<TimeFrame, TimeFrame_Surrogate>
{
    public TimeFrame ConvertFromSurrogate(in TimeFrame_Surrogate surrogate) => TimeFrame.Parse(surrogate.ShortName);

    public TimeFrame_Surrogate ConvertToSurrogate(in TimeFrame value)
    {
        return new TimeFrame_Surrogate
        {
            ShortName = value.ToShortString(),
        };
    }
}

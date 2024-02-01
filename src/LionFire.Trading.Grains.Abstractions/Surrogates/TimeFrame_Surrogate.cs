using Orleans;

namespace LionFire.Trading.Orleans_.Surrogates;

[GenerateSerializer]
[Alias("timeframe")]
public struct TimeFrame_Surrogate
{
    [Id(0)]
    public string ShortName { get; set; }
}
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


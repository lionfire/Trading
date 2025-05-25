using LionFire.Serialization;
using System.ComponentModel;

namespace LionFire.Trading.Automation;

[TypeConverter(typeof(ParsableConverter<BacktestReference>))]
public record BacktestReference(int BatchId, long BacktestId) : IParsableSlim<BacktestReference>
{
    public const char Separator = '-';
    public static BacktestReference Parse(string s)
    {
        var parts = s.Split('-');
        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid format: {s}");
        }
        return new BacktestReference(int.Parse(parts[0]), long.Parse(parts[1]));
    }

    public override string ToString()
    {
        return $"{BatchId}{Separator}{BacktestId}";
    }
}

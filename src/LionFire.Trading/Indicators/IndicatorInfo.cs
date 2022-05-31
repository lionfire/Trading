using LionFire.Structures;

namespace LionFire.Trading.Indicators;

public class IndicatorInfo  : IKeyed
{
    public IndicatorInfo(string abbreviation, string name = null, string longName = null, string description = null, IEnumerable<string> tags = null, Type type = null)
    {
        Key = abbreviation.ToLowerInvariant();
        Abbreviation = abbreviation;
        Name = name;
        LongName = longName;
        Description = description;
        if (tags != null) { Tags = new HashSet<string>(tags); }
        Type = type;
    }

    public string Key { get; set; }
    public string Abbreviation { get; set; }
    public string Name { get; set; }
    public string LongName { get; set; }
    public string? Description { get; set; }

    public HashSet<string>? Tags { get; set; }

    public Type Type { get; set; }
}


